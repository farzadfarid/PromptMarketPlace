using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using PromptMarketPlace.Models;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Services;

public class AiService : IAiService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<AiService> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public AiService(IHttpClientFactory httpFactory, ILogger<AiService> logger)
    {
        _httpFactory = httpFactory;
        _logger = logger;
    }

    public async Task<AiResponse> RunAsync(AiModel model, string? apiKey, string? systemContext,
        string prompt, OutputType outputType, List<string>? inputImageUrls = null)
    {
        return outputType switch
        {
            OutputType.Text or OutputType.Code or OutputType.Form
                => await RunChatCompletionAsync(model, apiKey, systemContext, prompt, inputImageUrls),
            OutputType.Image
                => await RunImageGenerationAsync(model, apiKey, prompt),
            OutputType.Video
                => await RunVideoGenerationAsync(model, apiKey, prompt, inputImageUrls),
            OutputType.Audio
                => await RunAudioGenerationAsync(model, apiKey, prompt),
            _ => AiResponse.Fail("نوع خروجی پشتیبانی نمی‌شود.")
        };
    }

    private async Task<AiResponse> RunChatCompletionAsync(AiModel model, string? apiKey,
        string? systemContext, string prompt, List<string>? inputImageUrls = null)
    {
        try
        {
            var client = BuildClient(model.Provider?.BaseUrl ?? throw new InvalidOperationException("مدل هوش مصنوعی به سرویس‌دهنده متصل نیست."), apiKey);

            var messages = new List<object>();
            if (!string.IsNullOrWhiteSpace(systemContext))
                messages.Add(new { role = "system", content = systemContext });

            // Build multimodal content when images are provided (vision API)
            object userContent;
            if (inputImageUrls != null && inputImageUrls.Count > 0)
            {
                var parts = new List<object> { new { type = "text", text = prompt } };
                foreach (var url in inputImageUrls)
                    parts.Add(new { type = "image_url", image_url = new { url } });
                userContent = parts;
            }
            else
            {
                userContent = prompt;
            }
            messages.Add(new { role = "user", content = userContent });

            var body = JsonSerializer.Serialize(new
            {
                model = model.ModelId,
                messages,
                max_tokens = model.MaxTokens ?? 2048
            });

            var request = BuildRequest(HttpMethod.Post, "chat/completions",
                new StringContent(body, Encoding.UTF8, "application/json"), apiKey);
            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("AI chat error {Status}: {Body}", response.StatusCode, json);
                var errMsg = TryExtractErrorMessage(json);
                return AiResponse.Fail($"خطا از سرویس AI ({(int)response.StatusCode}): {errMsg}");
            }

            var node = JsonNode.Parse(json);
            var text = node?["choices"]?[0]?["message"]?["content"]?.GetValue<string>() ?? "";

            // ── token count: try API usage first, fall back to char-based estimate ──
            var usage = node?["usage"];
            var tokens = usage?["total_tokens"]?.GetValue<int>()
                      ?? ((usage?["prompt_tokens"]?.GetValue<int>() ?? 0)
                        + (usage?["completion_tokens"]?.GetValue<int>() ?? 0));

            if (tokens == 0)
            {
                // provider did not return usage — estimate from actual content
                var inputChars  = (systemContext?.Length ?? 0) + prompt.Length;
                var outputChars = text.Length;
                tokens = (int)Math.Round((inputChars + outputChars) / 3.5);
            }

            _logger.LogInformation("Tokens for model {Model}: {Tokens} (api={ApiTokens})",
                model.ModelId, tokens, usage != null);

            return new AiResponse { IsSuccess = true, Text = text, TokensUsed = tokens };
        }
        catch (TaskCanceledException)
        {
            return AiResponse.Fail("زمان انتظار به پایان رسید. لطفاً مجدد تلاش کنید.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chat completion failed for model {ModelId}", model.ModelId);
            return AiResponse.Fail("خطا در ارتباط با سرویس AI.");
        }
    }

    private async Task<AiResponse> RunImageGenerationAsync(AiModel model, string? apiKey, string prompt)
    {
        var baseUrl = model.Provider?.BaseUrl ?? throw new InvalidOperationException("مدل هوش مصنوعی به سرویس‌دهنده متصل نیست.");
        try
        {
            var client = BuildClient(baseUrl, apiKey);

            // ── تلاش اول: endpoint استاندارد OpenAI ─────────────────
            var body = JsonSerializer.Serialize(new { model = model.ModelId, prompt, n = 1 });
            var request = BuildRequest(HttpMethod.Post, "images/generations",
                new StringContent(body, Encoding.UTF8, "application/json"), apiKey);
            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var node = JsonNode.Parse(json);
                var imageUrl = node?["data"]?[0]?["url"]?.GetValue<string>()
                            ?? node?["data"]?[0]?["b64_json"]?.GetValue<string>();

                if (!string.IsNullOrEmpty(imageUrl))
                    return AiResponse.SuccessImage(
                        imageUrl.StartsWith("data:") ? imageUrl : imageUrl);

                return AiResponse.Fail("تصویر از API دریافت نشد.");
            }

            // ── fallback: هر خطای 4xx (غیر از 401) → امتحان chat/completions ─
            // ChatQT و برخی سرویس‌دهنده‌ها endpoint مجزا ندارند؛ مدل‌های تصویری را
            // از chat/completions ارائه می‌دهند. 401 خطای واقعی API Key است.
            var statusCode = (int)response.StatusCode;
            if (statusCode >= 400 && statusCode < 500 && statusCode != 401)
            {
                _logger.LogInformation(
                    "images/generations returned {Status} for {Model} — falling back to chat/completions. Body: {Body}",
                    response.StatusCode, model.ModelId, json[..Math.Min(json.Length, 300)]);
                return await RunImageViaChatAsync(model, apiKey, prompt, baseUrl);
            }

            _logger.LogWarning("Image generation error {Status}: {Body}", response.StatusCode, json);
            var errMsg = TryExtractErrorMessage(json);
            return AiResponse.Fail($"خطا در تولید تصویر ({(int)response.StatusCode}): {errMsg}");
        }
        catch (TaskCanceledException)
        {
            return AiResponse.Fail("زمان انتظار به پایان رسید.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Image generation failed for model {ModelId}", model.ModelId);
            return AiResponse.Fail("خطا در تولید تصویر.");
        }
    }

    private async Task<AiResponse> RunImageViaChatAsync(AiModel model, string? apiKey, string prompt, string baseUrl)
    {
        try
        {
            var client = BuildClient(baseUrl, apiKey);

            // اطمینان از اینکه prompt صریحاً درخواست تولید تصویر دارد
            var messages = new List<object>
            {
                new { role = "user", content = prompt }
            };

            var body = JsonSerializer.Serialize(new
            {
                model = model.ModelId,
                messages,
                max_tokens = 4096
            });

            var request = BuildRequest(HttpMethod.Post, "chat/completions",
                new StringContent(body, Encoding.UTF8, "application/json"), apiKey);
            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var snippet = json[..Math.Min(json.Length, 600)];
                _logger.LogWarning("Image-via-chat error {Status} model={Model}: {Body}",
                    response.StatusCode, model.ModelId, snippet);
                var errDetail = TryExtractErrorMessage(json);
                // نمایش جزئیات کامل برای تشخیص مشکل
                return AiResponse.Fail($"خطا در تولید تصویر ({(int)response.StatusCode}): {errDetail} | پاسخ: {snippet[..Math.Min(snippet.Length, 200)]}");
            }

            var node = JsonNode.Parse(json);
            string? imageUrl = null;

            // ── ۱. ChatQT: choices[0].message.images[0].image_url.url ──────────
            var imagesArr = node?["choices"]?[0]?["message"]?["images"]?.AsArray();
            if (imagesArr != null && imagesArr.Count > 0)
                imageUrl = imagesArr[0]?["image_url"]?["url"]?.GetValue<string>();

            // ── ۲. OpenAI multimodal content array ────────────────────────────
            if (string.IsNullOrEmpty(imageUrl))
            {
                var content = node?["choices"]?[0]?["message"]?["content"];
                if (content?.GetValueKind() == System.Text.Json.JsonValueKind.Array)
                {
                    foreach (var part in content.AsArray())
                    {
                        var partType = part?["type"]?.GetValue<string>();
                        if (partType == "image_url")
                            imageUrl = part?["image_url"]?["url"]?.GetValue<string>();
                        else if (partType == "image")
                            imageUrl = part?["source"]?["url"]?.GetValue<string>()
                                    ?? part?["url"]?.GetValue<string>();
                        if (!string.IsNullOrEmpty(imageUrl)) break;
                    }
                }
                else
                {
                    var text = content?.GetValue<string>() ?? "";
                    imageUrl = ExtractImageUrlFromText(text);
                }
            }

            if (string.IsNullOrEmpty(imageUrl))
            {
                var rawSnippet = json[..Math.Min(json.Length, 600)];
                _logger.LogWarning("No image URL found in chat response for {Model}: {Snippet}", model.ModelId, rawSnippet);
                return AiResponse.Fail($"تصویر از API دریافت نشد. پاسخ سرور: {rawSnippet}");
            }

            return AiResponse.SuccessImage(imageUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Image-via-chat failed for {ModelId}", model.ModelId);
            return AiResponse.Fail("خطا در تولید تصویر.");
        }
    }

    private static string? ExtractImageUrlFromText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;

        // markdown: ![alt](url)
        var mdMatch = System.Text.RegularExpressions.Regex.Match(text, @"!\[.*?\]\((https?://\S+?)\)");
        if (mdMatch.Success) return mdMatch.Groups[1].Value.TrimEnd(')');

        // plain URL on its own line or after colon
        var urlMatch = System.Text.RegularExpressions.Regex.Match(text,
            @"https?://\S+\.(?:png|jpg|jpeg|webp|gif)(\?\S*)?", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (urlMatch.Success) return urlMatch.Value;

        // any URL that looks like a CDN image path
        var anyUrl = System.Text.RegularExpressions.Regex.Match(text, @"https?://\S{20,}");
        if (anyUrl.Success) return anyUrl.Value;

        return null;
    }

    private async Task<AiResponse> RunVideoGenerationAsync(AiModel model, string? apiKey, string prompt, List<string>? inputImageUrls = null)
    {
        var baseUrl = model.Provider?.BaseUrl ?? throw new InvalidOperationException("مدل هوش مصنوعی به سرویس‌دهنده متصل نیست.");
        try
        {
            var client = BuildClient(baseUrl, apiKey);

            object requestPayload = (inputImageUrls != null && inputImageUrls.Count > 0)
                ? new { model = model.ModelId, prompt, images = inputImageUrls }
                : (object)new { model = model.ModelId, prompt };
            var body = JsonSerializer.Serialize(requestPayload);
            var request = BuildRequest(HttpMethod.Post, "video/generations",
                new StringContent(body, Encoding.UTF8, "application/json"), apiKey);
            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var node = JsonNode.Parse(json);
                var videoUrl = node?["data"]?[0]?["url"]?.GetValue<string>();
                if (!string.IsNullOrEmpty(videoUrl))
                    return new AiResponse { IsSuccess = true, VideoUrl = videoUrl };

                var jobId = node?["id"]?.GetValue<string>();
                if (!string.IsNullOrEmpty(jobId))
                    return new AiResponse { IsSuccess = true, JobId = jobId };

                return AiResponse.Fail("پاسخ نامعتبر از سرویس ویدیو.");
            }

            // fallback: 4xx (not 401) → try chat/completions
            var statusCode = (int)response.StatusCode;
            if (statusCode >= 400 && statusCode < 500 && statusCode != 401)
            {
                _logger.LogInformation(
                    "video/generations returned {Status} for {Model} — falling back to chat/completions",
                    response.StatusCode, model.ModelId);
                return await RunVideoViaChatAsync(model, apiKey, prompt, baseUrl);
            }

            return AiResponse.Fail($"خطا در تولید ویدیو ({statusCode}): {TryExtractErrorMessage(json)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Video generation failed for model {ModelId}", model.ModelId);
            return AiResponse.Fail("خطا در تولید ویدیو.");
        }
    }

    private async Task<AiResponse> RunVideoViaChatAsync(AiModel model, string? apiKey, string prompt, string baseUrl)
    {
        try
        {
            var client = BuildClient(baseUrl, apiKey);
            var body = JsonSerializer.Serialize(new
            {
                model = model.ModelId,
                messages = new[] { new { role = "user", content = prompt } },
                max_tokens = 4096
            });

            var request = BuildRequest(HttpMethod.Post, "chat/completions",
                new StringContent(body, Encoding.UTF8, "application/json"), apiKey);
            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var errDetail = TryExtractErrorMessage(json);
                return AiResponse.Fail($"خطا در تولید ویدیو ({(int)response.StatusCode}): {errDetail}");
            }

            var node = JsonNode.Parse(json);

            // ChatQT style: choices[0].message.videos[0].video_url.url
            var videosArr = node?["choices"]?[0]?["message"]?["videos"]?.AsArray();
            if (videosArr != null && videosArr.Count > 0)
            {
                var videoUrl = videosArr[0]?["video_url"]?["url"]?.GetValue<string>();
                if (!string.IsNullOrEmpty(videoUrl))
                    return new AiResponse { IsSuccess = true, VideoUrl = videoUrl };
            }

            // plain text with URL
            var text = node?["choices"]?[0]?["message"]?["content"]?.GetValue<string>() ?? "";
            var extracted = ExtractVideoUrlFromText(text);
            if (!string.IsNullOrEmpty(extracted))
                return new AiResponse { IsSuccess = true, VideoUrl = extracted };

            var snippet = json[..Math.Min(json.Length, 400)];
            _logger.LogWarning("No video URL in chat response for {Model}: {Snippet}", model.ModelId, snippet);
            return AiResponse.Fail($"ویدیو از API دریافت نشد. پاسخ سرور: {snippet}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Video-via-chat failed for {ModelId}", model.ModelId);
            return AiResponse.Fail("خطا در تولید ویدیو.");
        }
    }

    private static string? ExtractVideoUrlFromText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var match = System.Text.RegularExpressions.Regex.Match(text,
            @"https?://\S+\.(?:mp4|webm|mov|avi)(\?\S*)?",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return match.Success ? match.Value : null;
    }

    private async Task<AiResponse> RunAudioGenerationAsync(AiModel model, string? apiKey, string prompt)
    {
        try
        {
            var client = BuildClient(model.Provider?.BaseUrl ?? throw new InvalidOperationException("مدل هوش مصنوعی به سرویس‌دهنده متصل نیست."), apiKey);

            var body = JsonSerializer.Serialize(new
            {
                model = model.ModelId,
                input = prompt
            });

            var request = BuildRequest(HttpMethod.Post, "audio/speech",
                new StringContent(body, Encoding.UTF8, "application/json"), apiKey);
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return AiResponse.Fail($"خطا در تولید صدا: {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();
            var node = JsonNode.Parse(json);
            var audioUrl = node?["data"]?[0]?["url"]?.GetValue<string>();

            return string.IsNullOrEmpty(audioUrl)
                ? AiResponse.Fail("صدا از API دریافت نشد.")
                : new AiResponse { IsSuccess = true, AudioUrl = audioUrl };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Audio generation failed for model {ModelId}", model.ModelId);
            return AiResponse.Fail("خطا در تولید صدا.");
        }
    }

    public async Task<AiResponse> CheckVideoStatusAsync(string jobId, AiModel model, string? apiKey)
    {
        try
        {
            var client = BuildClient(model.Provider?.BaseUrl ?? throw new InvalidOperationException("مدل هوش مصنوعی به سرویس‌دهنده متصل نیست."), apiKey);
            var response = await client.GetAsync($"video/generations/{jobId}");
            var json = await response.Content.ReadAsStringAsync();
            var node = JsonNode.Parse(json);

            var status = node?["status"]?.GetValue<string>();
            if (status == "completed")
            {
                var videoUrl = node?["data"]?[0]?["url"]?.GetValue<string>();
                return new AiResponse { IsSuccess = true, VideoUrl = videoUrl };
            }

            if (status == "failed")
                return AiResponse.Fail("تولید ویدیو با خطا مواجه شد.");

            // still processing
            return new AiResponse { IsSuccess = false, JobId = jobId, ErrorMessage = "در حال پردازش..." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Video status check failed for job {JobId}", jobId);
            return AiResponse.Fail("خطا در بررسی وضعیت ویدیو.");
        }
    }

    private static string TryExtractErrorMessage(string json)
    {
        try
        {
            var node = System.Text.Json.Nodes.JsonNode.Parse(json);
            return node?["error"]?["message"]?.GetValue<string>()
                ?? node?["message"]?.GetValue<string>()
                ?? json[..Math.Min(json.Length, 200)];
        }
        catch { return json[..Math.Min(json.Length, 200)]; }
    }

    private HttpClient BuildClient(string baseUrl, string? apiKey)
    {
        var client = _httpFactory.CreateClient();
        client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + '/');
        client.Timeout = TimeSpan.FromSeconds(120);
        return client;
    }

    private HttpRequestMessage BuildRequest(HttpMethod method, string url, HttpContent? content, string? apiKey)
    {
        var request = new HttpRequestMessage(method, url) { Content = content };
        if (!string.IsNullOrWhiteSpace(apiKey))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Headers.Add("HTTP-Referer", "https://promptmarket.ir");
        request.Headers.Add("X-Title", "PromptMarket");
        return request;
    }
}
