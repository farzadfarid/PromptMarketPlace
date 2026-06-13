using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using PromptMarketPlace.Models;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Services.Strategies;

/// <summary>
/// Google AI Studio strategy.
/// Chat: Gemini generateContent API.
/// Image: Imagen 3 predict API (returns base64).
/// Video: Veo 2 predictLongRunning API (async operation polling).
/// Auth: X-goog-api-key header (not Bearer).
/// </summary>
public class GoogleGeminiStrategy : BaseProviderStrategy
{
    private readonly IStorageService _storage;

    public GoogleGeminiStrategy(IHttpClientFactory httpFactory, ILogger<GoogleGeminiStrategy> logger,
        IStorageService storage) : base(httpFactory, logger)
    {
        _storage = storage;
    }

    public override async Task<AiResponse> RunChatAsync(AiModel model, string? apiKey,
        string? systemContext, string prompt, List<string>? inputImageUrls = null)
    {
        var baseUrl = model.Provider?.BaseUrl
            ?? throw new InvalidOperationException("مدل هوش مصنوعی به سرویس‌دهنده متصل نیست.");
        try
        {
            var client = BuildClient(baseUrl);

            var userParts = new List<object> { new { text = prompt } };
            if (inputImageUrls != null)
            {
                foreach (var url in inputImageUrls)
                {
                    var isAudio = IsAudioUrl(url);
                    if (isAudio)
                    {
                        try
                        {
                            byte[] audioBytes;
                            string audioMime;
                            if (url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                            {
                                var comma = url.IndexOf(',');
                                audioBytes = Convert.FromBase64String(url[(comma + 1)..]);
                                var hdr = url[..comma].ToLowerInvariant();
                                audioMime = hdr.Contains("wav") ? "audio/wav"
                                          : hdr.Contains("mp4") || hdr.Contains("m4a") ? "audio/mp4"
                                          : hdr.Contains("ogg") ? "audio/ogg" : "audio/mpeg";
                            }
                            else
                            {
                                var ac = HttpFactory.CreateClient();
                                ac.Timeout = TimeSpan.FromSeconds(60);
                                audioBytes = await ac.GetByteArrayAsync(url);
                                var ext = Path.GetExtension(url.Split('?')[0]).ToLowerInvariant();
                                audioMime = ext == ".wav" ? "audio/wav"
                                          : ext == ".m4a" ? "audio/mp4"
                                          : ext == ".ogg" ? "audio/ogg"
                                          : ext == ".flac" ? "audio/flac" : "audio/mpeg";
                            }
                            userParts.Add(new { inlineData = new { mimeType = audioMime, data = Convert.ToBase64String(audioBytes) } });
                        }
                        catch (Exception ex)
                        {
                            Logger.LogWarning(ex, "Gemini chat: could not load audio {Url}", url);
                        }
                    }
                    else if (url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                    {
                        var comma = url.IndexOf(',');
                        var header = url[..comma];
                        var mime = header.Contains("png") ? "image/png"
                                 : header.Contains("webp") ? "image/webp"
                                 : "image/jpeg";
                        var b64 = url[(comma + 1)..];
                        userParts.Add(new { inlineData = new { mimeType = mime, data = b64 } });
                    }
                }
            }

            var requestBody = new Dictionary<string, object>
            {
                ["contents"] = new[] { new { role = "user", parts = userParts } },
                ["generationConfig"] = new { maxOutputTokens = model.MaxTokens ?? 2048 }
            };

            if (!string.IsNullOrWhiteSpace(systemContext))
                requestBody["systemInstruction"] = new { parts = new[] { new { text = systemContext } } };

            var body = JsonSerializer.Serialize(requestBody);
            var request = BuildGoogleRequest(HttpMethod.Post,
                $"models/{model.ModelId}:generateContent",
                new StringContent(body, Encoding.UTF8, "application/json"),
                apiKey);

            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Gemini chat error {Status}: {Body}", response.StatusCode, json);
                return AiResponse.Fail($"خطا از Gemini ({(int)response.StatusCode}): {TryExtractGeminiError(json)}");
            }

            var node = System.Text.Json.Nodes.JsonNode.Parse(json);
            var text = node?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.GetValue<string>() ?? "";
            var tokens = node?["usageMetadata"]?["totalTokenCount"]?.GetValue<int>() ?? 0;

            if (tokens == 0)
            {
                var inputChars = (systemContext?.Length ?? 0) + prompt.Length;
                tokens = (int)Math.Round((inputChars + text.Length) / 3.5);
            }

            return new AiResponse { IsSuccess = true, Text = text, TokensUsed = tokens };
        }
        catch (TaskCanceledException)
        {
            return AiResponse.Fail("زمان انتظار به پایان رسید. لطفاً مجدد تلاش کنید.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Gemini chat failed for model {ModelId}", model.ModelId);
            return AiResponse.Fail("خطا در ارتباط با Gemini.");
        }
    }

    public override async Task<AiResponse> RunImageAsync(AiModel model, string? apiKey,
        string prompt, List<string>? inputImageUrls = null)
    {
        var baseUrl = model.Provider?.BaseUrl
            ?? throw new InvalidOperationException("مدل هوش مصنوعی به سرویس‌دهنده متصل نیست.");

        // Gemini models use generateContent; Imagen models use predict
        if (model.ModelId.Contains("gemini", StringComparison.OrdinalIgnoreCase))
            return await RunGeminiImageAsync(model, apiKey, prompt, inputImageUrls, baseUrl);

        try
        {
            var client = BuildClient(baseUrl);

            var requestBody = new
            {
                instances = new[] { new { prompt } },
                parameters = new { sampleCount = 1 }
            };

            var body = JsonSerializer.Serialize(requestBody);
            var request = BuildGoogleRequest(HttpMethod.Post,
                $"models/{model.ModelId}:predict",
                new StringContent(body, Encoding.UTF8, "application/json"),
                apiKey);

            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Imagen error {Status}: {Body}", response.StatusCode, json);
                return AiResponse.Fail($"خطا در تولید تصویر ({(int)response.StatusCode}): {TryExtractGeminiError(json)}");
            }

            var node = System.Text.Json.Nodes.JsonNode.Parse(json);
            var b64 = node?["predictions"]?[0]?["bytesBase64Encoded"]?.GetValue<string>();
            if (string.IsNullOrEmpty(b64))
                return AiResponse.Fail("تصویر از Imagen دریافت نشد.");

            var bytes = Convert.FromBase64String(b64);
            var localPath = await _storage.SaveBytesAsync(bytes, "image", ".png");
            return AiResponse.SuccessImage(localPath);
        }
        catch (TaskCanceledException)
        {
            return AiResponse.Fail("زمان انتظار به پایان رسید.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Imagen generation failed for model {ModelId}", model.ModelId);
            return AiResponse.Fail("خطا در تولید تصویر.");
        }
    }

    private async Task<AiResponse> RunGeminiImageAsync(AiModel model, string? apiKey,
        string prompt, List<string>? inputImageUrls, string baseUrl)
    {
        try
        {
            var client = BuildClient(baseUrl);

            var parts = new List<object> { new { text = prompt } };

            // Attach input images for editing mode
            if (inputImageUrls != null && inputImageUrls.Count > 0)
            {
                foreach (var url in inputImageUrls)
                {
                    byte[]? imgBytes = null;
                    string? mime = null;

                    if (url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                    {
                        var comma = url.IndexOf(',');
                        var header = url[..comma];
                        mime = header.Contains("png") ? "image/png"
                             : header.Contains("webp") ? "image/webp"
                             : "image/jpeg";
                        imgBytes = Convert.FromBase64String(url[(comma + 1)..]);
                    }
                    else
                    {
                        try
                        {
                            var imgClient = HttpFactory.CreateClient();
                            imgClient.Timeout = TimeSpan.FromSeconds(20);
                            imgBytes = await imgClient.GetByteArrayAsync(url);
                            mime = imgBytes.Length > 3 && imgBytes[0] == 0x89 ? "image/png" : "image/jpeg";
                        }
                        catch (Exception ex)
                        {
                            Logger.LogWarning(ex, "Gemini image: could not load input image {Url}", url);
                        }
                    }

                    if (imgBytes != null && mime != null)
                        parts.Add(new { inlineData = new { mimeType = mime, data = Convert.ToBase64String(imgBytes) } });
                }
            }

            var requestBody = new
            {
                contents = new[] { new { parts } },
                generationConfig = new { responseModalities = new[] { "image", "text" } }
            };

            var body = JsonSerializer.Serialize(requestBody);
            var request = BuildGoogleRequest(HttpMethod.Post,
                $"models/{model.ModelId}:generateContent",
                new StringContent(body, Encoding.UTF8, "application/json"),
                apiKey);

            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Gemini image error {Status}: {Body}", response.StatusCode, json);
                return AiResponse.Fail($"خطا در تولید تصویر ({(int)response.StatusCode}): {TryExtractGeminiError(json)}");
            }

            var node = System.Text.Json.Nodes.JsonNode.Parse(json);
            var partsNode = node?["candidates"]?[0]?["content"]?["parts"];
            if (partsNode == null)
                return AiResponse.Fail("پاسخ تصویر از Gemini دریافت نشد.");

            foreach (var part in partsNode.AsArray())
            {
                var b64 = part?["inlineData"]?["data"]?.GetValue<string>();
                if (!string.IsNullOrEmpty(b64))
                {
                    var imgMime = part?["inlineData"]?["mimeType"]?.GetValue<string>() ?? "image/png";
                    var ext = imgMime.Contains("jpeg") ? ".jpg" : ".png";
                    var imgBytes = Convert.FromBase64String(b64);
                    var localPath = await _storage.SaveBytesAsync(imgBytes, "image", ext);
                    return AiResponse.SuccessImage(localPath);
                }
            }

            return AiResponse.Fail("تصویر در پاسخ Gemini یافت نشد.");
        }
        catch (TaskCanceledException)
        {
            return AiResponse.Fail("زمان انتظار به پایان رسید.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Gemini image generation failed for model {ModelId}", model.ModelId);
            return AiResponse.Fail("خطا در تولید تصویر.");
        }
    }

    public override async Task<AiResponse> RunVideoAsync(AiModel model, string? apiKey,
        string prompt, List<string>? inputImageUrls = null)
    {
        var baseUrl = model.Provider?.BaseUrl
            ?? throw new InvalidOperationException("مدل هوش مصنوعی به سرویس‌دهنده متصل نیست.");
        try
        {
            var client = BuildVideoClient(baseUrl);
            var (cleanPrompt, seconds, aspectRatio, personGen) = ExtractVideoParams(prompt);

            var parameters = new Dictionary<string, object>
            {
                ["durationSeconds"] = int.TryParse(seconds, out var sec) ? sec : 8,
                ["personGeneration"] = personGen
            };
            if (!string.IsNullOrEmpty(aspectRatio))
                parameters["aspectRatio"] = aspectRatio;

            var instances = new List<object> { new { prompt = cleanPrompt } };
            if (inputImageUrls != null && inputImageUrls.Count > 0)
            {
                var imgUrl = inputImageUrls[0];
                try
                {
                    byte[] imgBytes;
                    if (imgUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                    {
                        var comma = imgUrl.IndexOf(',');
                        imgBytes = Convert.FromBase64String(imgUrl[(comma + 1)..]);
                    }
                    else
                    {
                        var imgClient = HttpFactory.CreateClient();
                        imgClient.Timeout = TimeSpan.FromSeconds(20);
                        imgBytes = await imgClient.GetByteArrayAsync(imgUrl);
                    }
                    var mime = DetectVeoMimeType(imgBytes);
                    if (mime == null)
                    {
                        Logger.LogWarning("Veo(Gemini): unsupported image format (not JPEG/PNG) for {Url} — skipping", imgUrl);
                    }
                    else
                    {
                        Logger.LogInformation("Veo(Gemini): reference image attached from {Url} ({Bytes} bytes, {Mime})", imgUrl, imgBytes.Length, mime);
                        instances = new List<object> { new { prompt = cleanPrompt, image = new { bytesBase64Encoded = Convert.ToBase64String(imgBytes), mimeType = mime } } };
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Veo(Gemini): could not load reference image {Url}, proceeding without it", imgUrl);
                }
            }

            var requestBody = new { instances, parameters };
            var body = JsonSerializer.Serialize(requestBody);
            var request = BuildGoogleRequest(HttpMethod.Post,
                $"models/{model.ModelId}:predictLongRunning",
                new StringContent(body, Encoding.UTF8, "application/json"),
                apiKey);

            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var sc = (int)response.StatusCode;
                if (sc is 401 or 403)
                    return AiResponse.Fail($"خطای احراز هویت ({sc}). API Key را بررسی کنید.");
                return AiResponse.Fail($"خطا در شروع تولید ویدیو ({sc}): {TryExtractGeminiError(json)}");
            }

            var node = System.Text.Json.Nodes.JsonNode.Parse(json);
            var operationName = node?["name"]?.GetValue<string>();
            if (string.IsNullOrEmpty(operationName))
                return AiResponse.Fail("شناسه عملیات از Veo دریافت نشد.");

            Logger.LogInformation("Veo operation started: {OperationName}", operationName);
            return await PollVeoOperationAsync(operationName, baseUrl, apiKey, client);
        }
        catch (TaskCanceledException)
        {
            return AiResponse.Fail("زمان انتظار برای تولید ویدیو به پایان رسید. لطفاً مجدد تلاش کنید.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Veo video generation failed for model {ModelId}", model.ModelId);
            return AiResponse.Fail("خطا در تولید ویدیو.");
        }
    }

    public override async Task<AiResponse> CheckVideoStatusAsync(string jobId, AiModel model, string? apiKey)
    {
        var baseUrl = model.Provider?.BaseUrl
            ?? throw new InvalidOperationException("مدل هوش مصنوعی به سرویس‌دهنده متصل نیست.");
        try
        {
            var client = BuildClient(baseUrl);
            var operationName = jobId.StartsWith("google:") ? jobId[7..] : jobId;
            var request = BuildGoogleRequest(HttpMethod.Get, operationName, null, apiKey);
            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            var node = System.Text.Json.Nodes.JsonNode.Parse(json);

            var done = node?["done"]?.GetValue<bool>() ?? false;
            if (!done)
                return new AiResponse { IsSuccess = false, JobId = jobId, ErrorMessage = "در حال پردازش..." };

            if (node?["error"] != null)
            {
                var err = node?["error"]?["message"]?.GetValue<string>() ?? "تولید ویدیو با خطا مواجه شد.";
                return AiResponse.Fail(err);
            }

            var videoUri = node?["response"]?["generateVideoResponse"]?["generatedSamples"]?[0]?["video"]?["uri"]?.GetValue<string>();
            if (string.IsNullOrEmpty(videoUri))
                return AiResponse.Fail("لینک ویدیو در پاسخ یافت نشد.");

            var localPath = await DownloadGoogleVideoAsync(videoUri, apiKey);
            return localPath != null
                ? new AiResponse { IsSuccess = true, VideoUrl = localPath }
                : AiResponse.Fail("دانلود ویدیو از Google ناموفق بود.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Veo status check failed for job {JobId}", jobId);
            return AiResponse.Fail("خطا در بررسی وضعیت ویدیو.");
        }
    }

    private async Task<AiResponse> PollVeoOperationAsync(string operationName, string baseUrl,
        string? apiKey, HttpClient client)
    {
        const int maxAttempts = 40;

        for (int i = 0; i < maxAttempts; i++)
        {
            if (i > 0) await Task.Delay(TimeSpan.FromSeconds(15));
            try
            {
                var req = BuildGoogleRequest(HttpMethod.Get, operationName, null, apiKey);
                var resp = await client.SendAsync(req);
                if (!resp.IsSuccessStatusCode) continue;

                var respJson = await resp.Content.ReadAsStringAsync();
                var node = System.Text.Json.Nodes.JsonNode.Parse(respJson);
                var done = node?["done"]?.GetValue<bool>() ?? false;
                Logger.LogInformation("Veo operation {OperationName}: done={Done}", operationName, done);

                if (!done) continue;

                if (node?["error"] != null)
                {
                    var err = node?["error"]?["message"]?.GetValue<string>() ?? "تولید ویدیو با خطا مواجه شد.";
                    return AiResponse.Fail(err);
                }

                var videoUri = node?["response"]?["generateVideoResponse"]?["generatedSamples"]?[0]?["video"]?["uri"]?.GetValue<string>();
                if (string.IsNullOrEmpty(videoUri))
                    return AiResponse.Fail("ویدیو تولید شد اما لینک دریافت نشد.");

                // Download video from Google Files API and save locally
                var localPath = await DownloadGoogleVideoAsync(videoUri, apiKey);
                return localPath != null
                    ? new AiResponse { IsSuccess = true, VideoUrl = localPath }
                    : AiResponse.Fail("دانلود ویدیو از Google ناموفق بود.");
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Poll attempt {Attempt} failed for Veo operation {OperationName}", i + 1, operationName);
            }
        }

        return AiResponse.Fail("زمان انتظار برای تولید ویدیو به پایان رسید (۱۰ دقیقه).");
    }

    private async Task<string?> DownloadGoogleVideoAsync(string uri, string? apiKey)
    {
        try
        {
            var downloadClient = HttpFactory.CreateClient();
            downloadClient.Timeout = TimeSpan.FromMinutes(5);

            // Google Files API: add :download?alt=media to get the raw bytes
            var downloadUrl = uri.Contains("generativelanguage.googleapis.com") && uri.Contains("/files/")
                ? uri.Split('?')[0] + ":download?alt=media"
                : uri;

            var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
            if (!string.IsNullOrWhiteSpace(apiKey))
                request.Headers.TryAddWithoutValidation("X-goog-api-key", apiKey);

            var response = await downloadClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Google video download failed: {Status} from {Url}", response.StatusCode, downloadUrl);
                return null;
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "video/mp4";
            var ext = contentType.Contains("webm") ? ".webm" : ".mp4";
            return await _storage.SaveBytesAsync(bytes, "video", ext);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to download Google video from {Uri}", uri);
            return null;
        }
    }

    private static HttpRequestMessage BuildGoogleRequest(HttpMethod method, string url,
        HttpContent? content, string? apiKey)
    {
        var request = new HttpRequestMessage(method, url) { Content = content };
        if (!string.IsNullOrWhiteSpace(apiKey))
            request.Headers.TryAddWithoutValidation("X-goog-api-key", apiKey);
        return request;
    }

    private static (string cleanPrompt, string seconds, string? aspectRatio, string personGeneration) ExtractVideoParams(string prompt)
    {
        var seconds = "8";
        string? aspectRatio = null;
        var personGeneration = "dont_allow";

        var secMatch = Regex.Match(prompt, @"\[seconds:(\d+)\]");
        if (secMatch.Success)
        {
            seconds = secMatch.Groups[1].Value;
            prompt = prompt.Replace(secMatch.Value, "").Trim();
        }

        var aspectMatch = Regex.Match(prompt, @"\[aspect:([^\]]+)\]");
        if (aspectMatch.Success)
        {
            aspectRatio = aspectMatch.Groups[1].Value;
            prompt = prompt.Replace(aspectMatch.Value, "").Trim();
        }

        // [persons:dont_allow] | [persons:allow_adults] | [persons:allow_all]
        var personsMatch = Regex.Match(prompt, @"\[persons:([^\]]+)\]");
        if (personsMatch.Success)
        {
            personGeneration = personsMatch.Groups[1].Value.Trim();
            prompt = prompt.Replace(personsMatch.Value, "").Trim();
        }

        return (prompt.Trim(), seconds, aspectRatio, personGeneration);
    }

    private static bool IsAudioUrl(string url)
    {
        var path = url.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
            ? url[5..url.IndexOf(';')]
            : url.Split('?')[0].ToLowerInvariant();
        return path.EndsWith(".mp3") || path.EndsWith(".wav") || path.EndsWith(".m4a")
            || path.EndsWith(".ogg") || path.EndsWith(".flac") || path.EndsWith(".webm")
            || path.StartsWith("audio/");
    }

    private static string TryExtractGeminiError(string json)
    {
        try
        {
            var node = System.Text.Json.Nodes.JsonNode.Parse(json);
            return node?["error"]?["message"]?.GetValue<string>()
                ?? json[..Math.Min(json.Length, 200)];
        }
        catch { return json[..Math.Min(json.Length, 200)]; }
    }
}
