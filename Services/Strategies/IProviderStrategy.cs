using PromptMarketPlace.Models;
using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Services.Strategies;

public interface IProviderStrategy
{
    Task<AiResponse> RunChatAsync(AiModel model, string? apiKey, string? systemContext, string prompt, List<string>? inputImageUrls = null);
    Task<AiResponse> RunImageAsync(AiModel model, string? apiKey, string prompt, List<string>? inputImageUrls = null);
    Task<AiResponse> RunVideoAsync(AiModel model, string? apiKey, string prompt, List<string>? inputImageUrls = null);
    Task<AiResponse> RunAudioAsync(AiModel model, string? apiKey, string prompt);
    Task<AiResponse> CheckVideoStatusAsync(string jobId, AiModel model, string? apiKey);
}
