using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using PromptMarketPlace.Models;
using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Services.Strategies;

/// <summary>
/// Handles providers that expose the standard OpenAI-compatible API:
/// OpenAI, OpenRouter, Gemini (OpenAI-compat endpoint), etc.
/// </summary>
public class OpenAiCompatibleStrategy : BaseProviderStrategy
{
    public OpenAiCompatibleStrategy(IHttpClientFactory httpFactory,
        ILogger<OpenAiCompatibleStrategy> logger)
        : base(httpFactory, logger) { }

    public override Task<AiResponse> RunChatAsync(AiModel model, string? apiKey,
        string? systemContext, string prompt, List<string>? inputImageUrls = null)
        => RunOpenAiChatAsync(model, apiKey, systemContext, prompt, inputImageUrls);

    public override Task<AiResponse> RunImageAsync(AiModel model, string? apiKey,
        string prompt, List<string>? inputImageUrls = null)
        => RunOpenAiImageAsync(model, apiKey, prompt);

    public override async Task<AiResponse> RunVideoAsync(AiModel model, string? apiKey,
        string prompt, List<string>? inputImageUrls = null)
    {
        var baseUrl = model.Provider?.BaseUrl
            ?? throw new InvalidOperationException("مدل هوش مصنوعی به سرویس‌دهنده متصل نیست.");
        try
        {
            var client = BuildVideoClient(baseUrl);

            // Include reference image URL if provided (optional — only if API supports it)
            object requestObj = prompt.Length > 0 && inputImageUrls?.Count > 0
                ? new { model = model.ModelId, prompt, image_url = inputImageUrls[0] }
                : (object)new { model = model.ModelId, prompt };

            var body = JsonSerializer.Serialize(requestObj);
            var req = BuildRequest(HttpMethod.Post, "video/generations",
                new StringContent(body, Encoding.UTF8, "application/json"), apiKey);
            var resp = await client.SendAsync(req);

            if (resp.IsSuccessStatusCode)
                return await ParseVideoResponseAsync(await resp.Content.ReadAsStringAsync(),
                    jobId => PollVideoJobAsync(jobId, baseUrl, apiKey, client, "video/generations"));

            var sc = (int)resp.StatusCode;
            if (sc is 401 or 403)
                return AiResponse.Fail($"خطای احراز هویت ({sc}). API Key را بررسی کنید.");

            var errJson = await resp.Content.ReadAsStringAsync();
            return AiResponse.Fail($"خطا در تولید ویدیو ({sc}): {TryExtractErrorMessage(errJson)}");
        }
        catch (TaskCanceledException)
        {
            return AiResponse.Fail("زمان انتظار برای تولید ویدیو به پایان رسید. لطفاً مجدد تلاش کنید.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Video generation failed for model {ModelId}", model.ModelId);
            return AiResponse.Fail("خطا در تولید ویدیو.");
        }
    }

    public override async Task<AiResponse> RunAudioAsync(AiModel model, string? apiKey, string prompt)
    {
        try
        {
            var client = BuildClient(model.Provider?.BaseUrl
                ?? throw new InvalidOperationException("مدل هوش مصنوعی به سرویس‌دهنده متصل نیست."));

            var body = JsonSerializer.Serialize(new { model = model.ModelId, input = prompt });
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
            Logger.LogError(ex, "Audio generation failed for model {ModelId}", model.ModelId);
            return AiResponse.Fail("خطا در تولید صدا.");
        }
    }

    public override async Task<AiResponse> CheckVideoStatusAsync(string jobId, AiModel model, string? apiKey)
    {
        try
        {
            var baseUrl = model.Provider?.BaseUrl
                ?? throw new InvalidOperationException("مدل هوش مصنوعی به سرویس‌دهنده متصل نیست.");
            var client = BuildClient(baseUrl);
            var request = BuildRequest(HttpMethod.Get, $"video/generations/{jobId}", null, apiKey);
            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            var node = JsonNode.Parse(json);

            var status = node?["status"]?.GetValue<string>();
            if (status is "succeeded" or "completed")
            {
                var videoUrl = node?["generations"]?[0]?["url"]?.GetValue<string>()
                            ?? node?["data"]?[0]?["url"]?.GetValue<string>()
                            ?? $"{baseUrl.TrimEnd('/')}/video/generations/{jobId}/content";
                return new AiResponse { IsSuccess = true, VideoUrl = videoUrl };
            }
            if (status is "failed" or "error")
            {
                var err = node?["error"]?["message"]?.GetValue<string>() ?? "تولید ویدیو با خطا مواجه شد.";
                return AiResponse.Fail(err);
            }

            return new AiResponse { IsSuccess = false, JobId = jobId, ErrorMessage = "در حال پردازش..." };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Video status check failed for job {JobId}", jobId);
            return AiResponse.Fail("خطا در بررسی وضعیت ویدیو.");
        }
    }
}
