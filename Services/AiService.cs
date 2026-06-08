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
        string prompt, OutputType outputType)
    {
        return outputType switch
        {
            OutputType.Text or OutputType.Code or OutputType.Form
                => await RunChatCompletionAsync(model, apiKey, systemContext, prompt),
            OutputType.Image
                => await RunImageGenerationAsync(model, apiKey, prompt),
            OutputType.Video
                => await RunVideoGenerationAsync(model, apiKey, prompt),
            OutputType.Audio
                => await RunAudioGenerationAsync(model, apiKey, prompt),
            _ => AiResponse.Fail("نوع خروجی پشتیبانی نمی‌شود.")
        };
    }

    private async Task<AiResponse> RunChatCompletionAsync(AiModel model, string? apiKey,
        string? systemContext, string prompt)
    {
        try
        {
            var client = BuildClient(model.Provider?.BaseUrl ?? "https://openrouter.ai/api/v1", apiKey);

            var messages = new List<object>();
            if (!string.IsNullOrWhiteSpace(systemContext))
                messages.Add(new { role = "system", content = systemContext });
            messages.Add(new { role = "user", content = prompt });

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
                _logger.LogWarning("OpenRouter chat error {Status}: {Body}", response.StatusCode, json);
                var errMsg = TryExtractErrorMessage(json);
                return AiResponse.Fail($"خطا از سرویس AI ({(int)response.StatusCode}): {errMsg}");
            }

            var node = JsonNode.Parse(json);
            var text = node?["choices"]?[0]?["message"]?["content"]?.GetValue<string>() ?? "";
            var tokens = node?["usage"]?["total_tokens"]?.GetValue<int>() ?? 0;

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
        try
        {
            var client = BuildClient(model.Provider?.BaseUrl ?? "https://openrouter.ai/api/v1", apiKey);

            var body = JsonSerializer.Serialize(new
            {
                model = model.ModelId,
                prompt,
                n = 1
            });

            var request = BuildRequest(HttpMethod.Post, "images/generations",
                new StringContent(body, Encoding.UTF8, "application/json"), apiKey);
            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("OpenRouter image error {Status}: {Body}", response.StatusCode, json);
                return AiResponse.Fail($"خطا در تولید تصویر: {response.StatusCode}");
            }

            var node = JsonNode.Parse(json);
            var imageUrl = node?["data"]?[0]?["url"]?.GetValue<string>();

            if (string.IsNullOrEmpty(imageUrl))
                return AiResponse.Fail("تصویر از API دریافت نشد.");

            return AiResponse.SuccessImage(imageUrl);
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

    private async Task<AiResponse> RunVideoGenerationAsync(AiModel model, string? apiKey, string prompt)
    {
        // Video generation is async — submit job, return JobId for polling
        try
        {
            var client = BuildClient(model.Provider?.BaseUrl ?? "https://openrouter.ai/api/v1", apiKey);

            var body = JsonSerializer.Serialize(new
            {
                model = model.ModelId,
                prompt
            });

            var request = BuildRequest(HttpMethod.Post, "video/generations",
                new StringContent(body, Encoding.UTF8, "application/json"), apiKey);
            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return AiResponse.Fail($"خطا در تولید ویدیو: {response.StatusCode}");

            var node = JsonNode.Parse(json);

            // try to get direct URL first (sync models)
            var videoUrl = node?["data"]?[0]?["url"]?.GetValue<string>();
            if (!string.IsNullOrEmpty(videoUrl))
                return new AiResponse { IsSuccess = true, VideoUrl = videoUrl };

            // async job
            var jobId = node?["id"]?.GetValue<string>();
            if (!string.IsNullOrEmpty(jobId))
                return new AiResponse { IsSuccess = true, JobId = jobId };

            return AiResponse.Fail("پاسخ نامعتبر از سرویس ویدیو.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Video generation failed for model {ModelId}", model.ModelId);
            return AiResponse.Fail("خطا در تولید ویدیو.");
        }
    }

    private async Task<AiResponse> RunAudioGenerationAsync(AiModel model, string? apiKey, string prompt)
    {
        try
        {
            var client = BuildClient(model.Provider?.BaseUrl ?? "https://openrouter.ai/api/v1", apiKey);

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
            var client = BuildClient(model.Provider?.BaseUrl ?? "https://openrouter.ai/api/v1", apiKey);
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
        var client = _httpFactory.CreateClient("OpenRouter");
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
