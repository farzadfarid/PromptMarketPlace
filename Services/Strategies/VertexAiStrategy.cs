using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using PromptMarketPlace.Models;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Services.Strategies;

/// <summary>
/// Google Vertex AI strategy.
/// Auth: OAuth2 refresh token flow (no service account JSON key needed).
/// ApiKey field stores JSON: {"client_id":"...","client_secret":"...","refresh_token":"..."}
/// BaseUrl: https://us-central1-aiplatform.googleapis.com/v1/projects/{project}/locations/{region}
/// Video: Veo via predictLongRunning — polls operation until done, saves video locally.
/// </summary>
public class VertexAiStrategy : BaseProviderStrategy
{
    private readonly IStorageService _storage;
    private readonly IMemoryCache _cache;

    public VertexAiStrategy(IHttpClientFactory httpFactory, ILogger<VertexAiStrategy> logger,
        IStorageService storage, IMemoryCache cache) : base(httpFactory, logger)
    {
        _storage = storage;
        _cache = cache;
    }

    public override async Task<AiResponse> RunChatAsync(AiModel model, string? apiKey,
        string? systemContext, string prompt, List<string>? inputImageUrls = null)
    {
        var baseUrl = model.Provider?.BaseUrl
            ?? throw new InvalidOperationException("مدل هوش مصنوعی به سرویس‌دهنده متصل نیست.");
        try
        {
            var token = await GetAccessTokenAsync(apiKey);
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
                            Logger.LogWarning(ex, "Vertex AI chat: could not load audio {Url}", url);
                        }
                    }
                    else if (url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                    {
                        var comma = url.IndexOf(',');
                        var header = url[..comma];
                        var mime = header.Contains("png") ? "image/png"
                                 : header.Contains("webp") ? "image/webp"
                                 : "image/jpeg";
                        userParts.Add(new { inlineData = new { mimeType = mime, data = url[(comma + 1)..] } });
                    }
                }
            }

            var requestBody = new Dictionary<string, object>
            {
                ["contents"] = new[] { new { role = "user", parts = userParts } },
                ["generationConfig"] = new { maxOutputTokens = model.MaxTokens ?? 8192 }
            };
            if (!string.IsNullOrWhiteSpace(systemContext))
                requestBody["systemInstruction"] = new { parts = new[] { new { text = systemContext } } };

            var body = JsonSerializer.Serialize(requestBody);
            var request = BuildBearerRequest(HttpMethod.Post,
                $"publishers/google/models/{model.ModelId}:generateContent",
                new StringContent(body, Encoding.UTF8, "application/json"), token);

            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return AiResponse.Fail($"خطا از Vertex AI ({(int)response.StatusCode}): {TryExtractError(json)}");

            var node = System.Text.Json.Nodes.JsonNode.Parse(json);
            var text = node?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.GetValue<string>() ?? "";
            var tokens = node?["usageMetadata"]?["totalTokenCount"]?.GetValue<int>() ?? 0;
            if (tokens == 0)
                tokens = (int)Math.Round(((systemContext?.Length ?? 0) + prompt.Length + text.Length) / 3.5);

            return new AiResponse { IsSuccess = true, Text = text, TokensUsed = tokens };
        }
        catch (TaskCanceledException)
        {
            return AiResponse.Fail("زمان انتظار به پایان رسید.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Vertex AI chat failed for model {ModelId}", model.ModelId);
            return AiResponse.Fail("خطا در ارتباط با Vertex AI.");
        }
    }

    public override async Task<AiResponse> RunImageAsync(AiModel model, string? apiKey,
        string prompt, List<string>? inputImageUrls = null)
    {
        var baseUrl = model.Provider?.BaseUrl
            ?? throw new InvalidOperationException("مدل هوش مصنوعی به سرویس‌دهنده متصل نیست.");

        if (model.ModelId.Contains("gemini", StringComparison.OrdinalIgnoreCase))
            return await RunGeminiImageAsync(model, apiKey, prompt, inputImageUrls, baseUrl);

        try
        {
            var token = await GetAccessTokenAsync(apiKey);
            var client = BuildClient(baseUrl);

            var requestBody = new
            {
                instances = new[] { new { prompt } },
                parameters = new { sampleCount = 1 }
            };

            var body = JsonSerializer.Serialize(requestBody);
            var request = BuildBearerRequest(HttpMethod.Post,
                $"publishers/google/models/{model.ModelId}:predict",
                new StringContent(body, Encoding.UTF8, "application/json"), token);

            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return AiResponse.Fail($"خطا در تولید تصویر ({(int)response.StatusCode}): {TryExtractError(json)}");

            var node = System.Text.Json.Nodes.JsonNode.Parse(json);
            var b64 = node?["predictions"]?[0]?["bytesBase64Encoded"]?.GetValue<string>();
            if (string.IsNullOrEmpty(b64))
                return AiResponse.Fail("تصویر از Vertex AI دریافت نشد.");

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
            Logger.LogError(ex, "Vertex AI image failed for model {ModelId}", model.ModelId);
            return AiResponse.Fail("خطا در تولید تصویر.");
        }
    }

    private async Task<AiResponse> RunGeminiImageAsync(AiModel model, string? apiKey,
        string prompt, List<string>? inputImageUrls, string baseUrl)
    {
        try
        {
            var token = await GetAccessTokenAsync(apiKey);
            // Gemini 3 models require the global (non-regional) endpoint
            var geminiBaseUrl = System.Text.RegularExpressions.Regex.Replace(
                baseUrl, @"https://[a-z0-9-]+-aiplatform\.googleapis\.com",
                "https://aiplatform.googleapis.com");
            var client = BuildClient(geminiBaseUrl);

            var parts = new List<object> { new { text = prompt } };

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
                            Logger.LogWarning(ex, "Gemini image (Vertex): could not load input image {Url}", url);
                        }
                    }

                    if (imgBytes != null && mime != null)
                        parts.Add(new { inlineData = new { mimeType = mime, data = Convert.ToBase64String(imgBytes) } });
                }
            }

            var requestBody = new
            {
                contents = new[] { new { role = "user", parts } },
                generationConfig = new { responseModalities = new[] { "image", "text" } }
            };

            var body = JsonSerializer.Serialize(requestBody);
            var request = BuildBearerRequest(HttpMethod.Post,
                $"publishers/google/models/{model.ModelId}:generateContent",
                new StringContent(body, Encoding.UTF8, "application/json"), token);

            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Gemini image (Vertex) error {Status}: {Body}", response.StatusCode, json);
                return AiResponse.Fail($"خطا در تولید تصویر ({(int)response.StatusCode}): {TryExtractError(json)}");
            }

            var node = System.Text.Json.Nodes.JsonNode.Parse(json);
            var partsNode = node?["candidates"]?[0]?["content"]?["parts"];
            if (partsNode == null)
                return AiResponse.Fail("پاسخ تصویر از Gemini (Vertex) دریافت نشد.");

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

            return AiResponse.Fail("تصویر در پاسخ Gemini (Vertex) یافت نشد.");
        }
        catch (TaskCanceledException)
        {
            return AiResponse.Fail("زمان انتظار به پایان رسید.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Gemini image (Vertex) failed for model {ModelId}", model.ModelId);
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
            var token = await GetAccessTokenAsync(apiKey);
            var client = BuildVideoClient(baseUrl);
            var (cleanPrompt, seconds, aspectRatio, personGen) = ExtractVideoParams(prompt);

            var parameters = new Dictionary<string, object>
            {
                ["durationSeconds"] = int.TryParse(seconds, out var sec) ? sec : 8,
                ["personGeneration"] = personGen,
                ["sampleCount"] = 1
            };
            if (!string.IsNullOrEmpty(aspectRatio))
                parameters["aspectRatio"] = aspectRatio;

            // Fetch reference product image if provided and include in Veo request
            // Veo supports image-conditioning: the model uses the image as visual reference
            string? refImageB64 = null;
            string? refImageMime = null;
            if (inputImageUrls != null && inputImageUrls.Count > 0)
            {
                try
                {
                    byte[] imgBytes;
                    var imgUrl = inputImageUrls[0];
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

                    refImageMime = DetectVeoMimeType(imgBytes);
                    if (refImageMime == null)
                    {
                        Logger.LogWarning("Veo: unsupported image format (not JPEG/PNG) for {Url} — skipping", imgUrl);
                    }
                    else
                    {
                        refImageB64 = Convert.ToBase64String(imgBytes);
                        Logger.LogInformation("Veo: reference image attached ({Bytes} bytes, {Mime})", imgBytes.Length, refImageMime);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Veo: could not load reference image, proceeding without it");
                }
            }

            // Build instance — with or without reference image
            object instance = refImageB64 != null
                ? (object)new
                {
                    prompt = cleanPrompt,
                    image  = new { bytesBase64Encoded = refImageB64, mimeType = refImageMime }
                }
                : new { prompt = cleanPrompt };

            var requestBody = new { instances = new[] { instance }, parameters };
            var body = JsonSerializer.Serialize(requestBody);
            var request = BuildBearerRequest(HttpMethod.Post,
                $"publishers/google/models/{model.ModelId}:predictLongRunning",
                new StringContent(body, Encoding.UTF8, "application/json"), token);

            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var sc = (int)response.StatusCode;
                if (sc is 401 or 403)
                    return AiResponse.Fail($"خطای احراز هویت ({sc}). Refresh Token را بررسی کنید.");
                return AiResponse.Fail($"خطا در شروع تولید ویدیو ({sc}): {TryExtractError(json)}");
            }

            var node = System.Text.Json.Nodes.JsonNode.Parse(json);
            var operationName = node?["name"]?.GetValue<string>();
            if (string.IsNullOrEmpty(operationName))
                return AiResponse.Fail("شناسه عملیات از Vertex AI دریافت نشد.");

            Logger.LogInformation("Vertex AI Veo operation started: {OperationName}", operationName);
            return await PollVertexOperationAsync(operationName, apiKey, baseUrl);
        }
        catch (TaskCanceledException)
        {
            return AiResponse.Fail("زمان انتظار برای تولید ویدیو به پایان رسید.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Vertex AI Veo failed for model {ModelId}", model.ModelId);
            return AiResponse.Fail("خطا در تولید ویدیو.");
        }
    }

    public override async Task<AiResponse> CheckVideoStatusAsync(string jobId, AiModel model, string? apiKey)
    {
        try
        {
            var token = await GetAccessTokenAsync(apiKey);
            var client = BuildClient("https://us-central1-aiplatform.googleapis.com/v1/");
            var operationName = jobId.StartsWith("vertex:") ? jobId[7..] : jobId;
            var request = BuildBearerRequest(HttpMethod.Get, operationName, null, token);
            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            return await ParseVertexOperationAsync(json, apiKey);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Vertex AI status check failed for job {JobId}", jobId);
            return AiResponse.Fail("خطا در بررسی وضعیت ویدیو.");
        }
    }

    private async Task<AiResponse> PollVertexOperationAsync(string operationName, string? apiKey, string baseUrl)
    {
        const int maxAttempts = 60;

        // Veo uses fetchPredictOperation (POST) — not the generic LRO GET endpoint.
        // Operation name: projects/P/locations/L/publishers/google/models/M/operations/ID
        // fetchPredictOperation path: publishers/google/models/M:fetchPredictOperation
        var modelPathMatch = Regex.Match(operationName, @"(publishers/[^/]+/models/[^/]+)/operations/");
        var fetchEndpoint = modelPathMatch.Success
            ? $"{modelPathMatch.Groups[1].Value}:fetchPredictOperation"
            : null;

        // Standard LRO fallback (strips publishers/models segment)
        var lroPath = Regex.Replace(
            operationName,
            @"/publishers/[^/]+/models/[^/]+/operations/",
            "/operations/");

        Logger.LogInformation("Polling Vertex AI op. Original={Op} FetchEndpoint={Fetch} LroFallback={Lro}",
            operationName, fetchEndpoint ?? "(none)", lroPath);

        string? lastError = null;

        for (int i = 0; i < maxAttempts; i++)
        {
            if (i > 0) await Task.Delay(TimeSpan.FromSeconds(15));
            try
            {
                var token = await GetAccessTokenAsync(apiKey);
                HttpResponseMessage resp;
                string json;

                if (fetchEndpoint != null)
                {
                    // Primary: fetchPredictOperation (Veo-specific documented approach)
                    var fetchClient = BuildClient(baseUrl);
                    var body = JsonSerializer.Serialize(new { operationName });
                    var req = BuildBearerRequest(HttpMethod.Post, fetchEndpoint,
                        new StringContent(body, Encoding.UTF8, "application/json"), token);
                    resp = await fetchClient.SendAsync(req);
                    json = await resp.Content.ReadAsStringAsync();

                    if (!resp.IsSuccessStatusCode)
                    {
                        // Fallback: try standard LRO GET endpoint
                        var lroClient = HttpFactory.CreateClient();
                        lroClient.Timeout = TimeSpan.FromSeconds(30);
                        var lroReq = new HttpRequestMessage(HttpMethod.Get,
                            $"https://us-central1-aiplatform.googleapis.com/v1/{lroPath}");
                        lroReq.Headers.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                        var lroResp = await lroClient.SendAsync(lroReq);
                        if (lroResp.IsSuccessStatusCode)
                        {
                            resp = lroResp;
                            json = await resp.Content.ReadAsStringAsync();
                        }
                        else
                        {
                            lastError = $"fetchPredictOperation HTTP {(int)resp.StatusCode}: {json[..Math.Min(json.Length, 300)]}";
                            Logger.LogWarning("Vertex AI poll {Attempt} failed both endpoints: {Error}", i + 1, lastError);
                            continue;
                        }
                    }
                }
                else
                {
                    // Standard LRO only (operation name format did not include model path)
                    var pollClient = HttpFactory.CreateClient();
                    pollClient.Timeout = TimeSpan.FromSeconds(30);
                    var req = new HttpRequestMessage(HttpMethod.Get,
                        $"https://us-central1-aiplatform.googleapis.com/v1/{lroPath}");
                    req.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    resp = await pollClient.SendAsync(req);
                    json = await resp.Content.ReadAsStringAsync();

                    if (!resp.IsSuccessStatusCode)
                    {
                        lastError = $"HTTP {(int)resp.StatusCode}: {json[..Math.Min(json.Length, 300)]}";
                        Logger.LogWarning("Vertex AI poll {Attempt} failed: {Error}", i + 1, lastError);
                        continue;
                    }
                }

                var node = System.Text.Json.Nodes.JsonNode.Parse(json);
                var done = node?["done"]?.GetValue<bool>() ?? false;

                if (!done)
                {
                    Logger.LogInformation("Vertex AI still processing (attempt {Attempt}/{Max})", i + 1, maxAttempts);
                    continue;
                }

                return await ParseVertexOperationAsync(json, apiKey);
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                Logger.LogWarning(ex, "Poll attempt {Attempt} exception for op {Op}", i + 1, operationName);
            }
        }

        return AiResponse.Fail($"زمان انتظار برای تولید ویدیو به پایان رسید. آخرین خطا: {lastError ?? "نامشخص"}");
    }

    private async Task<AiResponse> ParseVertexOperationAsync(string json, string? apiKey)
    {
        var node = System.Text.Json.Nodes.JsonNode.Parse(json);
        var done = node?["done"]?.GetValue<bool>() ?? false;

        if (!done)
            return new AiResponse { IsSuccess = false, ErrorMessage = "در حال پردازش..." };

        if (node?["error"] != null)
        {
            var err = node?["error"]?["message"]?.GetValue<string>() ?? "تولید ویدیو با خطا مواجه شد.";
            return AiResponse.Fail(err);
        }

        Logger.LogInformation("Veo done response (first 2000 chars): {Json}", json[..Math.Min(json.Length, 2000)]);

        var response = node?["response"];

        // Check RAI safety filter — all samples were blocked
        var raiFiltered = response?["raiMediaFilteredCount"]?.GetValue<int>() ?? 0;
        if (raiFiltered > 0)
        {
            var reasons = response?["raiMediaFilteredReasons"]?.AsArray();
            var reason = reasons?.Count > 0
                ? reasons[0]?.GetValue<string>() ?? ""
                : "";
            // Extract the short human-readable part before the long support code list
            var shortReason = reason.Contains("Try rephrasing")
                ? "فیلتر محتوا: " + reason[..reason.IndexOf("Try rephrasing")].Trim() + " لطفاً prompt را تغییر دهید."
                : "ویدیو توسط فیلتر محتوای Google بلاک شد. لطفاً prompt را ساده‌تر کنید.";
            Logger.LogWarning("Veo RAI filtered {Count} video(s): {Reason}", raiFiltered, reason);
            return AiResponse.Fail(shortReason);
        }

        // Veo API nests result under generateVideoResponse (PredictLongRunningResponse type)
        var videoResponse = response?["generateVideoResponse"] ?? response;

        // Try all known array field names
        var videos = videoResponse?["generatedSamples"]?.AsArray()
                  ?? videoResponse?["videos"]?.AsArray()
                  ?? response?["predictions"]?.AsArray();

        if (videos != null && videos.Count > 0)
        {
            var first = videos[0];

            // Inline base64 — try direct and nested under "video"
            var b64 = first?["bytesBase64Encoded"]?.GetValue<string>()
                   ?? first?["video"]?["bytesBase64Encoded"]?.GetValue<string>();
            if (!string.IsNullOrEmpty(b64))
            {
                var bytes = Convert.FromBase64String(b64);
                var localPath = await _storage.SaveBytesAsync(bytes, "video", ".mp4");
                return new AiResponse { IsSuccess = true, VideoUrl = localPath };
            }

            // GCS URI — try all known field names at every nesting level
            var gcsUri = first?["gcsUri"]?.GetValue<string>()
                      ?? first?["uri"]?.GetValue<string>()
                      ?? first?["video"]?["uri"]?.GetValue<string>()
                      ?? first?["video"]?["gcsUri"]?.GetValue<string>()
                      ?? first?["videoUri"]?.GetValue<string>();
            if (!string.IsNullOrEmpty(gcsUri))
            {
                var localPath = await DownloadFromGcsAsync(gcsUri, apiKey);
                return localPath != null
                    ? new AiResponse { IsSuccess = true, VideoUrl = localPath }
                    : AiResponse.Fail($"دانلود ویدیو از GCS ناموفق بود: {gcsUri}");
            }

            // Log the first element to show exactly what fields we got
            Logger.LogWarning("Veo: videos[0] keys present but no URI/b64 found. Element: {Element}",
                first?.ToJsonString()[..Math.Min(first.ToJsonString().Length, 500)]);
        }

        // No videos array found — include response snippet in error for debugging
        var responseSnippet = json[..Math.Min(json.Length, 800)];
        Logger.LogWarning("Veo: could not locate video data. Full response: {Json}", json);
        return AiResponse.Fail($"فرمت پاسخ Veo ناشناخته. پاسخ سرور:\n{responseSnippet}");
    }

    private async Task<string?> DownloadFromGcsAsync(string gcsUri, string? apiKey)
    {
        try
        {
            // gs://bucket/path → https://storage.googleapis.com/bucket/path
            var httpUrl = gcsUri.Replace("gs://", "https://storage.googleapis.com/");
            var token = await GetAccessTokenAsync(apiKey);

            var client = HttpFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(5);
            var request = new HttpRequestMessage(HttpMethod.Get, httpUrl);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("GCS download failed: {Status} from {Url}", response.StatusCode, httpUrl);
                return null;
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();
            return await _storage.SaveBytesAsync(bytes, "video", ".mp4");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to download from GCS: {Uri}", gcsUri);
            return null;
        }
    }

    private async Task<string> GetAccessTokenAsync(string? apiKey)
    {
        var creds = ParseCredentials(apiKey);
        var cacheKey = $"vertex_token_{creds.ClientId[..20]}";

        if (_cache.TryGetValue(cacheKey, out string? cached))
            return cached!;

        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = creds.ClientId,
            ["client_secret"] = creds.ClientSecret,
            ["refresh_token"] = creds.RefreshToken,
            ["grant_type"] = "refresh_token"
        });

        var client = HttpFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(15);
        var response = await client.PostAsync("https://oauth2.googleapis.com/token", form);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"OAuth token refresh failed: {json[..Math.Min(json.Length, 200)]}");

        var node = System.Text.Json.Nodes.JsonNode.Parse(json);
        var token = node?["access_token"]?.GetValue<string>()
            ?? throw new InvalidOperationException("access_token not found in OAuth response");
        var expiresIn = node?["expires_in"]?.GetValue<int>() ?? 3600;

        _cache.Set(cacheKey, token, TimeSpan.FromSeconds(expiresIn - 60));
        Logger.LogInformation("Vertex AI: refreshed OAuth token (expires in {Seconds}s)", expiresIn);
        return token;
    }

    private static VertexCredentials ParseCredentials(string? apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Vertex AI credentials not configured. Store {\"client_id\":\"...\",\"client_secret\":\"...\",\"refresh_token\":\"...\"} in the API Key field.");
        try
        {
            var doc = JsonDocument.Parse(apiKey);
            return new VertexCredentials(
                doc.RootElement.GetProperty("client_id").GetString() ?? "",
                doc.RootElement.GetProperty("client_secret").GetString() ?? "",
                doc.RootElement.GetProperty("refresh_token").GetString() ?? "");
        }
        catch
        {
            throw new InvalidOperationException("Vertex AI API Key باید یک JSON با فیلدهای client_id، client_secret و refresh_token باشد.");
        }
    }

    private static HttpRequestMessage BuildBearerRequest(HttpMethod method, string url,
        HttpContent? content, string token)
    {
        var request = new HttpRequestMessage(method, url) { Content = content };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private static (string cleanPrompt, string seconds, string? aspectRatio, string personGeneration) ExtractVideoParams(string prompt)
    {
        var seconds = "8";
        string? aspectRatio = null;
        var personGeneration = "dont_allow";

        var secMatch = Regex.Match(prompt, @"\[seconds:(\d+)\]");
        if (secMatch.Success) { seconds = secMatch.Groups[1].Value; prompt = prompt.Replace(secMatch.Value, "").Trim(); }

        var aspectMatch = Regex.Match(prompt, @"\[aspect:([^\]]+)\]");
        if (aspectMatch.Success) { aspectRatio = aspectMatch.Groups[1].Value; prompt = prompt.Replace(aspectMatch.Value, "").Trim(); }

        // [persons:dont_allow] | [persons:allow_adults] | [persons:allow_all]
        var personsMatch = Regex.Match(prompt, @"\[persons:([^\]]+)\]");
        if (personsMatch.Success)
        {
            personGeneration = personsMatch.Groups[1].Value.Trim();
            prompt = prompt.Replace(personsMatch.Value, "").Trim();
        }

        return (prompt.Trim(), seconds, aspectRatio, personGeneration);
    }

    private static string TryExtractError(string json)
    {
        try
        {
            var node = System.Text.Json.Nodes.JsonNode.Parse(json);
            return node?["error"]?["message"]?.GetValue<string>()
                ?? json[..Math.Min(json.Length, 200)];
        }
        catch { return json[..Math.Min(json.Length, 200)]; }
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

    private record VertexCredentials(string ClientId, string ClientSecret, string RefreshToken);
}
