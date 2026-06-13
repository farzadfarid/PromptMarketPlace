using PromptMarketPlace.Models;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;
using PromptMarketPlace.Services.Interfaces;
using PromptMarketPlace.Services.Strategies;

namespace PromptMarketPlace.Services;

public class AiService : IAiService
{
    private readonly ProviderStrategyFactory _factory;
    private readonly ILogger<AiService> _logger;

    public AiService(ProviderStrategyFactory factory, ILogger<AiService> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public async Task<AiResponse> RunAsync(AiModel model, string? apiKey, string? systemContext,
        string prompt, OutputType outputType, List<string>? inputImageUrls = null)
    {
        if (model.Provider == null)
            return AiResponse.Fail("مدل به سرویس‌دهنده متصل نیست.");

        var strategy = _factory.Resolve(model.Provider.ProviderType);

        return outputType switch
        {
            OutputType.Text or OutputType.Code or OutputType.Form
                => await strategy.RunChatAsync(model, apiKey, systemContext, prompt, inputImageUrls),
            OutputType.Image
                => await strategy.RunImageAsync(model, apiKey, prompt, inputImageUrls),
            OutputType.Video
                => await strategy.RunVideoAsync(model, apiKey, prompt, inputImageUrls),
            OutputType.Audio
                => await strategy.RunAudioAsync(model, apiKey, prompt),
            _ => AiResponse.Fail("نوع خروجی پشتیبانی نمی‌شود.")
        };
    }

    public async Task<AiResponse> CheckVideoStatusAsync(string jobId, AiModel model, string? apiKey)
    {
        if (model.Provider == null)
            return AiResponse.Fail("مدل به سرویس‌دهنده متصل نیست.");

        var strategy = _factory.Resolve(model.Provider.ProviderType);
        return await strategy.CheckVideoStatusAsync(jobId, model, apiKey);
    }
}
