using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using PromptMarketPlace.Models;
using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Services.Strategies;

/// <summary>
/// ChatQT strategy.
/// Chat uses standard chat/completions.
/// Image and video use chat/completions with ChatQT-specific response parsing
/// (images/videos arrays in message object).
/// Audio is not supported.
/// </summary>
public class ChatQtStrategy : BaseProviderStrategy
{
    public ChatQtStrategy(IHttpClientFactory httpFactory, ILogger<ChatQtStrategy> logger)
        : base(httpFactory, logger) { }

    public override Task<AiResponse> RunChatAsync(AiModel model, string? apiKey,
        string? systemContext, string prompt, List<string>? inputImageUrls = null)
        => RunOpenAiChatAsync(model, apiKey, systemContext, prompt, inputImageUrls);

    public override async Task<AiResponse> RunImageAsync(AiModel model, string? apiKey,
        string prompt, List<string>? inputImageUrls = null)
    {
        var baseUrl = model.Provider?.BaseUrl
            ?? throw new InvalidOperationException("مدل هوش مصنوعی به سرویس‌دهنده متصل نیست.");
        try
        {
            var client = BuildClient(baseUrl);

            // Try standard images/generations first
            var bodyStd = JsonSerializer.Serialize(new { model = model.ModelId, prompt, n = 1 });
            var reqStd = BuildRequest(HttpMethod.Post, "images/generations",
                new StringContent(bodyStd, Encoding.UTF8, "application/json"), apiKey);
            var respStd = await client.SendAsync(reqStd);

            if (respStd.IsSuccessStatusCode)
            {
                var stdJson = await respStd.Content.ReadAsStringAsync();
                var stdNode = JsonNode.Parse(stdJson);
                var stdUrl = stdNode?["data"]?[0]?["url"]?.GetValue<string>();
                if (!string.IsNullOrEmpty(stdUrl))
                    return AiResponse.SuccessImage(stdUrl);
            }

            var sc = (int)respStd.StatusCode;
            if (sc is 401 or 403)
                return AiResponse.Fail($"خطای احراز هویت ({sc}). API Key را بررسی کنید.");

            // ChatQT serves image models via chat/completions
            Logger.LogInformation("images/generations → {Status} for {Model} — trying chat/completions", sc, model.ModelId);
            return await RunImageViaChatQtAsync(model, apiKey, prompt, baseUrl, client);
        }
        catch (TaskCanceledException)
        {
            return AiResponse.Fail("زمان انتظار به پایان رسید.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "ChatQT image generation failed for {ModelId}", model.ModelId);
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
            var client = BuildClient(baseUrl);

            // Use multimodal message content when a reference image is supplied
            object messageContent = inputImageUrls?.Count > 0
                ? (object)new object[]
                {
                    new { type = "image_url", image_url = new { url = inputImageUrls[0] } },
                    new { type = "text", text = prompt }
                }
                : prompt;

            var body = JsonSerializer.Serialize(new
            {
                model = model.ModelId,
                messages = new[] { new { role = "user", content = messageContent } },
                max_tokens = 4096
            });

            var request = BuildRequest(HttpMethod.Post, "chat/completions",
                new StringContent(body, Encoding.UTF8, "application/json"), apiKey);
            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return AiResponse.Fail($"خطا در تولید ویدیو ({(int)response.StatusCode}): {TryExtractErrorMessage(json)}");

            var node = JsonNode.Parse(json);

            // ChatQT style: choices[0].message.videos[0].video_url.url
            var videosArr = node?["choices"]?[0]?["message"]?["videos"]?.AsArray();
            if (videosArr != null && videosArr.Count > 0)
            {
                var videoUrl = videosArr[0]?["video_url"]?["url"]?.GetValue<string>();
                if (!string.IsNullOrEmpty(videoUrl))
                    return new AiResponse { IsSuccess = true, VideoUrl = videoUrl };
            }

            var text = node?["choices"]?[0]?["message"]?["content"]?.GetValue<string>() ?? "";
            var extracted = ExtractVideoUrlFromText(text);
            if (!string.IsNullOrEmpty(extracted))
                return new AiResponse { IsSuccess = true, VideoUrl = extracted };

            var snippet = json[..Math.Min(json.Length, 400)];
            Logger.LogWarning("No video URL in ChatQT response for {Model}: {Snippet}", model.ModelId, snippet);
            return AiResponse.Fail($"ویدیو از API دریافت نشد. پاسخ سرور: {snippet}");
        }
        catch (TaskCanceledException)
        {
            return AiResponse.Fail("زمان انتظار برای تولید ویدیو به پایان رسید.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "ChatQT video generation failed for {ModelId}", model.ModelId);
            return AiResponse.Fail("خطا در تولید ویدیو.");
        }
    }

    private async Task<AiResponse> RunImageViaChatQtAsync(AiModel model, string? apiKey,
        string prompt, string baseUrl, HttpClient client)
    {
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
            return AiResponse.Fail($"خطا در تولید تصویر ({(int)response.StatusCode}): {TryExtractErrorMessage(json)}");

        var node = JsonNode.Parse(json);
        string? imageUrl = null;

        // ChatQT: choices[0].message.images[0].image_url.url
        var imagesArr = node?["choices"]?[0]?["message"]?["images"]?.AsArray();
        if (imagesArr != null && imagesArr.Count > 0)
            imageUrl = imagesArr[0]?["image_url"]?["url"]?.GetValue<string>();

        // Standard content array
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
                        imageUrl = part?["source"]?["url"]?.GetValue<string>() ?? part?["url"]?.GetValue<string>();
                    if (!string.IsNullOrEmpty(imageUrl)) break;
                }
            }
            else
            {
                imageUrl = ExtractImageUrlFromText(content?.GetValue<string>() ?? "");
            }
        }

        if (string.IsNullOrEmpty(imageUrl))
        {
            var snippet = json[..Math.Min(json.Length, 600)];
            Logger.LogWarning("No image URL in ChatQT response for {Model}: {Snippet}", model.ModelId, snippet);
            return AiResponse.Fail($"تصویر از API دریافت نشد. پاسخ سرور: {snippet}");
        }

        return AiResponse.SuccessImage(imageUrl);
    }
}
