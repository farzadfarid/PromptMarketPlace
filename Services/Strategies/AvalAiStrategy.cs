using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using PromptMarketPlace.Models;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Services.Strategies;

/// <summary>
/// AvalAI-specific strategy.
/// Chat and image use standard OpenAI-compatible endpoints.
/// Video uses AvalAI async job API (/videos).
/// Audio returns binary content — saved directly to disk.
/// </summary>
public class AvalAiStrategy : BaseProviderStrategy
{
    private readonly IWebHostEnvironment _env;
    private readonly IStorageService _storage;

    public AvalAiStrategy(IHttpClientFactory httpFactory, ILogger<AvalAiStrategy> logger,
        IWebHostEnvironment env, IStorageService storage)
        : base(httpFactory, logger)
    {
        _env = env;
        _storage = storage;
    }

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
            var (cleanPrompt, seconds, aspectRatio) = ExtractVideoParams(prompt);

            HttpContent body;
            if (inputImageUrls != null && inputImageUrls.Count > 0)
            {
                bool imageAttached = false;
                var form = new MultipartFormDataContent();
                form.Add(new StringContent(model.ModelId), "model");
                form.Add(new StringContent(cleanPrompt), "prompt");
                form.Add(new StringContent(seconds), "seconds");
                if (aspectRatio != null) form.Add(new StringContent(aspectRatio), "aspect_ratio");
                try
                {
                    byte[] imgBytes;
                    string ext;
                    var imgUrl = inputImageUrls[0];

                    if (imgUrl.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase) ||
                        imgUrl.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
                    {
                        var physPath = System.IO.Path.Combine(_env.WebRootPath, imgUrl.TrimStart('/'));
                        imgBytes = await System.IO.File.ReadAllBytesAsync(physPath);
                        ext = System.IO.Path.GetExtension(physPath);
                    }
                    else if (imgUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                    {
                        var comma = imgUrl.IndexOf(',');
                        var header = imgUrl[..comma];
                        ext = header.Contains("png") ? ".png" : header.Contains("webp") ? ".webp" : ".jpg";
                        imgBytes = Convert.FromBase64String(imgUrl[(comma + 1)..]);
                    }
                    else
                    {
                        var imgClient = HttpFactory.CreateClient();
                        imgClient.Timeout = TimeSpan.FromSeconds(30);
                        imgBytes = await imgClient.GetByteArrayAsync(imgUrl);
                        ext = System.IO.Path.GetExtension(new Uri(imgUrl).AbsolutePath);
                    }

                    if (string.IsNullOrEmpty(ext)) ext = ".jpg";
                    var mime = ext.ToLower() switch { ".png" => "image/png", ".webp" => "image/webp", _ => "image/jpeg" };
                    var imgContent = new ByteArrayContent(imgBytes);
                    imgContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mime);
                    form.Add(imgContent, "input_reference", $"image{ext}");
                    imageAttached = true;
                    Logger.LogInformation("Image reference attached for video generation ({Bytes} bytes)", imgBytes.Length);
                }
                catch (Exception ex) { Logger.LogWarning(ex, "Could not attach reference image — falling back to text-only"); }

                if (!imageAttached)
                {
                    var fallback = new Dictionary<string, string>
                    {
                        ["model"] = model.ModelId, ["prompt"] = cleanPrompt, ["seconds"] = seconds
                    };
                    if (aspectRatio != null) fallback["aspect_ratio"] = aspectRatio;
                    body = new StringContent(JsonSerializer.Serialize(fallback), Encoding.UTF8, "application/json");
                }
                else body = form;
            }
            else
            {
                var requestObj = new Dictionary<string, string>
                {
                    ["model"] = model.ModelId, ["prompt"] = cleanPrompt, ["seconds"] = seconds
                };
                if (aspectRatio != null) requestObj["aspect_ratio"] = aspectRatio;
                body = new StringContent(JsonSerializer.Serialize(requestObj), Encoding.UTF8, "application/json");
            }

            var req = BuildRequest(HttpMethod.Post, "videos", body, apiKey);
            var resp = await client.SendAsync(req);

            if (resp.IsSuccessStatusCode)
                return await ParseVideoResponseAsync(await resp.Content.ReadAsStringAsync(),
                    jobId => PollVideoJobAsync(jobId, baseUrl, apiKey, client, "videos"));

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
            Logger.LogError(ex, "AvalAI video generation failed for model {ModelId}", model.ModelId);
            return AiResponse.Fail("خطا در تولید ویدیو.");
        }
    }

    /// <summary>
    /// Extracts [seconds:N] and [aspect:W:H] markers from prompt before sending to AvalAI API.
    /// Defaults: 8 seconds, no aspect_ratio override.
    /// </summary>
    private static (string cleanPrompt, string seconds, string? aspectRatio) ExtractVideoParams(string prompt)
    {
        var seconds = "8";
        string? aspectRatio = null;

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

        return (prompt.Trim(), seconds, aspectRatio);
    }

    public override async Task<AiResponse> RunAudioAsync(AiModel model, string? apiKey, string prompt)
    {
        try
        {
            var client = BuildClient(model.Provider?.BaseUrl
                ?? throw new InvalidOperationException("مدل هوش مصنوعی به سرویس‌دهنده متصل نیست."));

            var body = JsonSerializer.Serialize(new { model = model.ModelId, input = prompt, voice = "nova" });
            var request = BuildRequest(HttpMethod.Post, "audio/speech",
                new StringContent(body, Encoding.UTF8, "application/json"), apiKey);
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return AiResponse.Fail($"خطا در تولید صدا ({(int)response.StatusCode}): {response.ReasonPhrase}");

            // AvalAI returns binary audio directly
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "audio/mpeg";
            var ext = contentType.Contains("wav") ? ".wav"
                    : contentType.Contains("ogg") ? ".ogg"
                    : ".mp3";
            var bytes = await response.Content.ReadAsByteArrayAsync();
            var localPath = await _storage.SaveBytesAsync(bytes, "audio", ext);

            return new AiResponse { IsSuccess = true, AudioUrl = localPath };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "AvalAI audio generation failed for model {ModelId}", model.ModelId);
            return AiResponse.Fail("خطا در تولید صدا.");
        }
    }

    public override async Task<AiResponse> CheckVideoStatusAsync(string jobId, AiModel model, string? apiKey)
    {
        try
        {
            var baseUrl = model.Provider?.BaseUrl
                ?? throw new InvalidOperationException("مدل هوش مصنوعی به سرویس‌دهنده متصل نیست.");

            // backward-compat: strip "avalai:" prefix if present
            var cleanId = jobId.StartsWith("avalai:") ? jobId[7..] : jobId;

            var client = BuildClient(baseUrl);
            var request = BuildRequest(HttpMethod.Get, $"videos/{cleanId}", null, apiKey);
            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            var node = System.Text.Json.Nodes.JsonNode.Parse(json);

            var status = node?["status"]?.GetValue<string>();
            if (status is "succeeded" or "completed")
            {
                var videoUrl = node?["generations"]?[0]?["url"]?.GetValue<string>()
                            ?? node?["data"]?[0]?["url"]?.GetValue<string>()
                            ?? $"{baseUrl.TrimEnd('/')}/videos/{cleanId}/content";
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
            Logger.LogError(ex, "AvalAI video status check failed for job {JobId}", jobId);
            return AiResponse.Fail("خطا در بررسی وضعیت ویدیو.");
        }
    }
}
