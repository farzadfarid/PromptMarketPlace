using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;

namespace PromptMarketPlace.Services.Interfaces;

public interface IAiProviderService
{
    Task<List<AiProvider>> GetAllProvidersAsync();
    Task<AiProvider?> GetProviderByIdAsync(int id);
    Task<AiProvider> CreateProviderAsync(string name, string baseUrl, string? apiKey, string? description,
        ProviderType providerType = ProviderType.OpenAiCompatible,
        string? balanceUrl = null, string? balanceJsonPath = null, string? balanceCurrency = null);
    Task UpdateProviderAsync(int id, string name, string baseUrl, string? newApiKey, string? description,
        ProviderType providerType = ProviderType.OpenAiCompatible,
        string? balanceUrl = null, string? balanceJsonPath = null, string? balanceCurrency = null);
    Task ToggleProviderActiveAsync(int id);
    Task DeleteProviderAsync(int id);

    // مسیریابی capability-based: برای هر نوع خروجی، provider و مدل فعال را برمی‌گرداند
    Task<(AiProvider? provider, AiModel? model, string? apiKey)> GetActiveSetupForOutputTypeAsync(OutputType outputType);
    // فعال/غیرفعال کردن یک provider برای یک capability (فقط یکی می‌تواند فعال باشد)
    Task SetCapabilityActiveAsync(int providerId, string capability, bool isActive);

    Task<List<AiModel>> GetAllModelsAsync(int? providerId = null, AiCapability? capability = null);
    Task<List<AiModel>> GetModelsForOutputTypeAsync(OutputType outputType);
    Task<AiModel?> GetModelByIdAsync(int id);
    Task<AiModel> CreateModelAsync(AiModel model);
    Task UpdateModelAsync(AiModel model);
    Task ToggleModelActiveAsync(int id);
    Task DeleteModelAsync(int id);
    Task SetDefaultModelAsync(int id);

    Task<string?> GetDecryptedApiKeyAsync(int providerId);
}
