using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Helpers;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Services;

public class AiProviderService : IAiProviderService
{
    private readonly ApplicationDbContext _db;
    private readonly IEncryptionService _encryption;

    public AiProviderService(ApplicationDbContext db, IEncryptionService encryption)
    {
        _db = db;
        _encryption = encryption;
    }

    public async Task<List<AiProvider>> GetAllProvidersAsync()
        => await _db.AiProviders.OrderBy(p => p.Name).ToListAsync();

    public async Task<AiProvider?> GetProviderByIdAsync(int id)
        => await _db.AiProviders.FindAsync(id);

    public async Task<AiProvider> CreateProviderAsync(string name, string baseUrl, string? apiKey, string? description)
    {
        var provider = new AiProvider
        {
            Name = name,
            BaseUrl = baseUrl,
            Description = description,
            ApiKeyEncrypted = string.IsNullOrWhiteSpace(apiKey) ? null : _encryption.Encrypt(apiKey),
            IsActive = true
        };
        _db.AiProviders.Add(provider);
        await _db.SaveChangesAsync();
        return provider;
    }

    public async Task UpdateProviderAsync(int id, string name, string baseUrl, string? newApiKey, string? description)
    {
        var provider = await _db.AiProviders.FindAsync(id)
            ?? throw new KeyNotFoundException($"Provider {id} not found.");

        provider.Name = name;
        provider.BaseUrl = baseUrl;
        provider.Description = description;

        if (!string.IsNullOrWhiteSpace(newApiKey))
            provider.ApiKeyEncrypted = _encryption.Encrypt(newApiKey);

        await _db.SaveChangesAsync();
    }

    public async Task ToggleProviderActiveAsync(int id)
    {
        var provider = await _db.AiProviders.FindAsync(id)
            ?? throw new KeyNotFoundException($"Provider {id} not found.");

        provider.IsActive = !provider.IsActive;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteProviderAsync(int id)
    {
        var provider = await _db.AiProviders.FindAsync(id)
            ?? throw new KeyNotFoundException($"Provider {id} not found.");

        var hasModels = await _db.AiModels.AnyAsync(m => m.AiProviderId == id);
        if (hasModels)
            throw new InvalidOperationException("ابتدا مدل‌های این سرویس‌دهنده را حذف کنید.");

        _db.AiProviders.Remove(provider);
        await _db.SaveChangesAsync();
    }

    public async Task<List<AiModel>> GetAllModelsAsync(int? providerId = null, AiCapability? capability = null)
    {
        var query = _db.AiModels.Include(m => m.Provider).AsQueryable();

        if (providerId.HasValue)
            query = query.Where(m => m.AiProviderId == providerId.Value);

        var models = await query.OrderBy(m => m.SortOrder).ThenBy(m => m.Name).ToListAsync();

        if (capability.HasValue)
        {
            var cap = capability.Value.ToString();
            models = models.Where(m =>
            {
                var caps = JsonSerializer.Deserialize<List<string>>(m.Capabilities) ?? new();
                return caps.Contains(cap);
            }).ToList();
        }

        return models;
    }

    public async Task<List<AiModel>> GetModelsForOutputTypeAsync(OutputType outputType)
    {
        var capability = OutputTypeCapabilityMap.ToCapability(outputType);
        return await GetAllModelsAsync(capability: capability);
    }

    public async Task<AiModel?> GetModelByIdAsync(int id)
        => await _db.AiModels.Include(m => m.Provider).FirstOrDefaultAsync(m => m.Id == id);

    public async Task<AiModel> CreateModelAsync(AiModel model)
    {
        _db.AiModels.Add(model);
        await _db.SaveChangesAsync();
        return model;
    }

    public async Task UpdateModelAsync(AiModel model)
    {
        _db.AiModels.Update(model);
        await _db.SaveChangesAsync();
    }

    public async Task ToggleModelActiveAsync(int id)
    {
        var model = await _db.AiModels.FindAsync(id)
            ?? throw new KeyNotFoundException($"Model {id} not found.");

        model.IsActive = !model.IsActive;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteModelAsync(int id)
    {
        var model = await _db.AiModels.FindAsync(id)
            ?? throw new KeyNotFoundException($"Model {id} not found.");

        var usedByApps = await _db.Apps.AnyAsync(a => a.AiModelId == id);
        if (usedByApps)
            throw new InvalidOperationException("این مدل توسط یک یا چند ابزار استفاده می‌شود. ابتدا مدل آن ابزارها را تغییر دهید.");

        _db.AiModels.Remove(model);
        await _db.SaveChangesAsync();
    }

    public async Task SetDefaultModelAsync(int id)
    {
        var model = await _db.AiModels.FindAsync(id)
            ?? throw new KeyNotFoundException($"Model {id} not found.");

        var capString = model.Capabilities;
        var siblings = await _db.AiModels
            .Where(m => m.Id != id && m.IsDefault)
            .ToListAsync();

        foreach (var s in siblings)
        {
            var sCaps = JsonSerializer.Deserialize<List<string>>(s.Capabilities) ?? new();
            var mCaps = JsonSerializer.Deserialize<List<string>>(capString) ?? new();
            if (sCaps.Intersect(mCaps).Any())
                s.IsDefault = false;
        }

        model.IsDefault = true;
        await _db.SaveChangesAsync();
    }

    public async Task<string?> GetDecryptedApiKeyAsync(int providerId)
    {
        var provider = await _db.AiProviders.FindAsync(providerId);
        if (provider?.ApiKeyEncrypted == null) return null;
        return _encryption.Decrypt(provider.ApiKeyEncrypted);
    }
}
