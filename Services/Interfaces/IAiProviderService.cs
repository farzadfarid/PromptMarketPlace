using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;

namespace PromptMarketPlace.Services.Interfaces;

public interface IAiProviderService
{
    Task<List<AiProvider>> GetAllProvidersAsync();
    Task<AiProvider?> GetProviderByIdAsync(int id);
    Task<AiProvider> CreateProviderAsync(string name, string baseUrl, string? apiKey, string? description);
    Task UpdateProviderAsync(int id, string name, string baseUrl, string? newApiKey, string? description);
    Task ToggleProviderActiveAsync(int id);

    Task<List<AiModel>> GetAllModelsAsync(int? providerId = null, AiCapability? capability = null);
    Task<List<AiModel>> GetModelsForOutputTypeAsync(OutputType outputType);
    Task<AiModel?> GetModelByIdAsync(int id);
    Task<AiModel> CreateModelAsync(AiModel model);
    Task UpdateModelAsync(AiModel model);
    Task ToggleModelActiveAsync(int id);
    Task SetDefaultModelAsync(int id);

    Task<string?> GetDecryptedApiKeyAsync(int providerId);
}
