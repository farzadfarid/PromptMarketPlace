using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using PromptMarketPlace.Models;
using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Services.Strategies;

/// <summary>
/// Anthropic Claude strategy.
/// Uses /messages endpoint with x-api-key header and anthropic-version header.
/// Image and video generation are not supported by Anthropic.
/// </summary>
public class AnthropicStrategy : BaseProviderStrategy
{
    private const string AnthropicVersion = "2023-06-01";

    public AnthropicStrategy(IHttpClientFactory httpFactory, ILogger<AnthropicStrategy> logger)
        : base(httpFactory, logger) { }

    public override async Task<AiResponse> RunChatAsync(AiModel model, string? apiKey,
        string? systemContext, string prompt, List<string>? inputImageUrls = null)
    {
        try
        {
            var client = BuildClient(model.Provider?.BaseUrl
                ?? throw new InvalidOperationException("مدل هوش مصنوعی به سرویس‌دهنده متصل نیست."));

            // Build message content (with vision if images provided)
            object userContent;
            if (inputImageUrls != null && inputImageUrls.Count > 0)
            {
                var parts = new List<object>();
                foreach (var url in inputImageUrls)
                    parts.Add(new { type = "image", source = new { type = "url", url } });
                parts.Add(new { type = "text", text = prompt });
                userContent = parts;
            }
            else
            {
                userContent = prompt;
            }

            var bodyObj = new Dictionary<string, object>
            {
                ["model"] = model.ModelId,
                ["max_tokens"] = model.MaxTokens ?? 2048,
                ["messages"] = new[] { new { role = "user", content = userContent } }
            };
            if (!string.IsNullOrWhiteSpace(systemContext))
                bodyObj["system"] = systemContext;

            var request = BuildAnthropicRequest(HttpMethod.Post, "messages",
                new StringContent(JsonSerializer.Serialize(bodyObj), Encoding.UTF8, "application/json"), apiKey);

            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("Anthropic chat error {Status}: {Body}", response.StatusCode, json);
                return AiResponse.Fail($"خطا از Anthropic ({(int)response.StatusCode}): {TryExtractErrorMessage(json)}");
            }

            var node = JsonNode.Parse(json);
            var text = node?["content"]?[0]?["text"]?.GetValue<string>() ?? "";

            var usage = node?["usage"];
            var inputTokens = usage?["input_tokens"]?.GetValue<int>() ?? 0;
            var outputTokens = usage?["output_tokens"]?.GetValue<int>() ?? 0;
            var tokens = inputTokens + outputTokens;
            if (tokens == 0)
                tokens = (int)Math.Round(((systemContext?.Length ?? 0) + prompt.Length + text.Length) / 3.5);

            return new AiResponse { IsSuccess = true, Text = text, TokensUsed = tokens };
        }
        catch (TaskCanceledException)
        {
            return AiResponse.Fail("زمان انتظار به پایان رسید. لطفاً مجدد تلاش کنید.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Anthropic chat failed for model {ModelId}", model.ModelId);
            return AiResponse.Fail("خطا در ارتباط با Anthropic.");
        }
    }

    public override Task<AiResponse> RunImageAsync(AiModel model, string? apiKey,
        string prompt, List<string>? inputImageUrls = null)
        => Task.FromResult(AiResponse.Fail("Anthropic از تولید تصویر پشتیبانی نمی‌کند."));

    private static HttpRequestMessage BuildAnthropicRequest(HttpMethod method, string url,
        HttpContent? content, string? apiKey)
    {
        var request = new HttpRequestMessage(method, url) { Content = content };
        if (!string.IsNullOrWhiteSpace(apiKey))
            request.Headers.TryAddWithoutValidation("x-api-key", apiKey);
        request.Headers.TryAddWithoutValidation("anthropic-version", AnthropicVersion);
        request.Headers.TryAddWithoutValidation("HTTP-Referer", "https://promptmarket.ir");
        request.Headers.TryAddWithoutValidation("X-Title", "PromptMarket");
        return request;
    }
}
