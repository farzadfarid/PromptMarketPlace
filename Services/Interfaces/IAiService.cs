using PromptMarketPlace.Models;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;

namespace PromptMarketPlace.Services.Interfaces;

public interface IAiService
{
    Task<AiResponse> RunAsync(AiModel model, string? apiKey, string? systemContext, string prompt, OutputType outputType, List<string>? inputImageUrls = null);
    Task<AiResponse> CheckVideoStatusAsync(string jobId, AiModel model, string? apiKey);
}
