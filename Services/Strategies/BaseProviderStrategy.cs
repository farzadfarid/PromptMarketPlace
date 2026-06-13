using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using PromptMarketPlace.Models;
using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Services.Strategies;

public abstract class BaseProviderStrategy : IProviderStrategy
{
    protected readonly IHttpClientFactory HttpFactory;
    protected readonly ILogger Logger;

    protected BaseProviderStrategy(IHttpClientFactory httpFactory, ILogger logger)
    {
        HttpFactory = httpFactory;
        Logger = logger;
    }

    // ── Abstract: each strategy must implement ────────────────────────────────

    public abstract Task<AiResponse> RunChatAsync(AiModel model, string? apiKey, string? systemContext,
        string prompt, List<string>? inputImageUrls = null);

    public abstract Task<AiResponse> RunImageAsync(AiModel model, string? apiKey, string prompt,
        List<string>? inputImageUrls = null);

    // ── Virtual defaults: "not supported" (override in strategies that support them) ─

    public virtual Task<AiResponse> RunVideoAsync(AiModel model, string? apiKey, string prompt,
        List<string>? inputImageUrls = null)
        => Task.FromResult(AiResponse.Fail("این سرویس‌دهنده از تولید ویدیو پشتیبانی نمی‌کند."));

    public virtual Task<AiResponse> RunAudioAsync(AiModel model, string? apiKey, string prompt)
        => Task.FromResult(AiResponse.Fail("این سرویس‌دهنده از تولید صدا پشتیبانی نمی‌کند."));

    public virtual Task<AiResponse> CheckVideoStatusAsync(string jobId, AiModel model, string? apiKey)
        => Task.FromResult(AiResponse.Fail("بررسی وضعیت ویدیو برای این سرویس‌دهنده پشتیبانی نمی‌شود."));

    // ── Shared protected helpers ──────────────────────────────────────────────

    protected HttpClient BuildClient(string baseUrl)
    {
        var client = HttpFactory.CreateClient();
        client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + '/');
        client.Timeout = TimeSpan.FromSeconds(120);
        return client;
    }

    protected HttpClient BuildVideoClient(string baseUrl)
    {
        var client = HttpFactory.CreateClient();
        client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + '/');
        client.Timeout = TimeSpan.FromMinutes(12);
        return client;
    }

    protected static HttpRequestMessage BuildRequest(HttpMethod method, string url,
        HttpContent? content, string? apiKey)
    {
        var request = new HttpRequestMessage(method, url) { Content = content };
        if (!string.IsNullOrWhiteSpace(apiKey))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Headers.TryAddWithoutValidation("HTTP-Referer", "https://promptmarket.ir");
        request.Headers.TryAddWithoutValidation("X-Title", "PromptMarket");
        return request;
    }

    protected static string TryExtractErrorMessage(string json)
    {
        try
        {
            var node = JsonNode.Parse(json);
            return node?["error"]?["message"]?.GetValue<string>()
                ?? node?["message"]?.GetValue<string>()
                ?? json[..Math.Min(json.Length, 200)];
        }
        catch { return json[..Math.Min(json.Length, 200)]; }
    }

    protected static string? ExtractImageUrlFromText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var mdMatch = System.Text.RegularExpressions.Regex.Match(text, @"!\[.*?\]\((https?://\S+?)\)");
        if (mdMatch.Success) return mdMatch.Groups[1].Value.TrimEnd(')');
        var urlMatch = System.Text.RegularExpressions.Regex.Match(text,
            @"https?://\S+\.(?:png|jpg|jpeg|webp|gif)(\?\S*)?",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (urlMatch.Success) return urlMatch.Value;
        var anyUrl = System.Text.RegularExpressions.Regex.Match(text, @"https?://\S{20,}");
        return anyUrl.Success ? anyUrl.Value : null;
    }

    protected static string? ExtractVideoUrlFromText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var match = System.Text.RegularExpressions.Regex.Match(text,
            @"https?://\S+\.(?:mp4|webm|mov|avi)(\?\S*)?",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return match.Success ? match.Value : null;
    }

    // ── Reusable OpenAI-compatible chat (used by OpenAI, AvalAI, ChatQT) ─────

    protected async Task<AiResponse> RunOpenAiChatAsync(AiModel model, string? apiKey,
        string? systemContext, string prompt, List<string>? inputImageUrls = null)
    {
        try
        {
            var client = BuildClient(model.Provider?.BaseUrl
                ?? throw new InvalidOperationException("مدل هوش مصنوعی به سرویس‌دهنده متصل نیست."));

            var messages = new List<object>();
            if (!string.IsNullOrWhiteSpace(systemContext))
                messages.Add(new { role = "system", content = systemContext });

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
                Logger.LogWarning("AI chat error {Status}: {Body}", response.StatusCode, json);
                return AiResponse.Fail($"خطا از سرویس AI ({(int)response.StatusCode}): {TryExtractErrorMessage(json)}");
            }

            var node = JsonNode.Parse(json);
            var text = node?["choices"]?[0]?["message"]?["content"]?.GetValue<string>() ?? "";

            var usage = node?["usage"];
            var tokens = usage?["total_tokens"]?.GetValue<int>()
                      ?? ((usage?["prompt_tokens"]?.GetValue<int>() ?? 0)
                        + (usage?["completion_tokens"]?.GetValue<int>() ?? 0));

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
            Logger.LogError(ex, "Chat completion failed for model {ModelId}", model.ModelId);
            return AiResponse.Fail("خطا در ارتباط با سرویس AI.");
        }
    }

    // ── Reusable OpenAI-compatible image generation (used by OpenAI, AvalAI) ─

    protected async Task<AiResponse> RunOpenAiImageAsync(AiModel model, string? apiKey, string prompt)
    {
        var baseUrl = model.Provider?.BaseUrl
            ?? throw new InvalidOperationException("مدل هوش مصنوعی به سرویس‌دهنده متصل نیست.");
        try
        {
            var client = BuildClient(baseUrl);
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
                    return AiResponse.SuccessImage(imageUrl);
                return AiResponse.Fail("تصویر از API دریافت نشد.");
            }

            Logger.LogWarning("Image generation error {Status}: {Body}", response.StatusCode, json);
            return AiResponse.Fail($"خطا در تولید تصویر ({(int)response.StatusCode}): {TryExtractErrorMessage(json)}");
        }
        catch (TaskCanceledException)
        {
            return AiResponse.Fail("زمان انتظار به پایان رسید.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Image generation failed for model {ModelId}", model.ModelId);
            return AiResponse.Fail("خطا در تولید تصویر.");
        }
    }

    // ── Image format detection (Veo only accepts JPEG or PNG) ────────────────

    /// <summary>
    /// Returns "image/jpeg" or "image/png" from magic bytes, or null for unsupported formats.
    /// Veo rejects WEBP, GIF, BMP etc., so callers must skip the image when null is returned.
    /// </summary>
    protected static string? DetectVeoMimeType(byte[] bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
            return "image/jpeg";
        if (bytes.Length >= 4 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
            return "image/png";
        return null; // WEBP, GIF, BMP, HEIC, etc. — not supported by Veo
    }

    // ── Shared video polling (used by AvalAI and OpenAI-compatible) ──────────

    protected static async Task<AiResponse> ParseVideoResponseAsync(string json,
        Func<string, Task<AiResponse>> pollFunc)
    {
        var node = JsonNode.Parse(json);
        var videoUrl = node?["data"]?[0]?["url"]?.GetValue<string>()
                    ?? node?["generations"]?[0]?["url"]?.GetValue<string>();
        if (!string.IsNullOrEmpty(videoUrl))
            return new AiResponse { IsSuccess = true, VideoUrl = videoUrl };

        var jobId = node?["id"]?.GetValue<string>();
        if (string.IsNullOrEmpty(jobId))
            return AiResponse.Fail("پاسخ نامعتبر از سرویس ویدیو.");

        var status = node?["status"]?.GetValue<string>();
        if (status is "failed" or "error")
        {
            var err = node?["error"]?["message"]?.GetValue<string>() ?? "تولید ویدیو با خطا مواجه شد.";
            return AiResponse.Fail(err);
        }

        return await pollFunc(jobId);
    }

    protected async Task<AiResponse> PollVideoJobAsync(string jobId, string baseUrl,
        string? apiKey, HttpClient client, string jobEndpointPrefix)
    {
        const int maxAttempts = 40;
        Logger.LogInformation("Video job {JobId} queued — polling via {Prefix}", jobId, jobEndpointPrefix);

        for (int i = 0; i < maxAttempts; i++)
        {
            if (i > 0) await Task.Delay(TimeSpan.FromSeconds(15));
            try
            {
                var req = BuildRequest(HttpMethod.Get, $"{jobEndpointPrefix}/{jobId}", null, apiKey);
                var resp = await client.SendAsync(req);
                if (!resp.IsSuccessStatusCode) continue;

                var respJson = await resp.Content.ReadAsStringAsync();
                var node = JsonNode.Parse(respJson);
                var status = node?["status"]?.GetValue<string>();
                Logger.LogInformation("Video job {JobId}: {Status}", jobId, status);

                if (status is "succeeded" or "completed")
                {
                    var videoUrl = node?["generations"]?[0]?["url"]?.GetValue<string>()
                                ?? node?["data"]?[0]?["url"]?.GetValue<string>();
                    if (!string.IsNullOrEmpty(videoUrl))
                        return new AiResponse { IsSuccess = true, VideoUrl = videoUrl };

                    var contentUrl = $"{baseUrl.TrimEnd('/')}/{jobEndpointPrefix}/{jobId}/content";
                    return new AiResponse { IsSuccess = true, VideoUrl = contentUrl };
                }
                if (status is "failed" or "error")
                {
                    var err = node?["error"]?["message"]?.GetValue<string>() ?? "تولید ویدیو با خطا مواجه شد.";
                    return AiResponse.Fail(err);
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Poll attempt {Attempt} failed for job {JobId}", i + 1, jobId);
            }
        }

        return AiResponse.Fail("زمان انتظار برای تولید ویدیو به پایان رسید (۱۰ دقیقه).");
    }
}
