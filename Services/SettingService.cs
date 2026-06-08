using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Services;

public class SettingService : ISettingService
{
    private readonly ApplicationDbContext _db;
    private readonly IEncryptionService _encryption;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public SettingService(ApplicationDbContext db, IEncryptionService encryption, IMemoryCache cache)
    {
        _db = db;
        _encryption = encryption;
        _cache = cache;
    }

    public async Task<string?> GetValueAsync(string key)
    {
        var cacheKey = $"setting:{key}";
        if (_cache.TryGetValue(cacheKey, out string? cached)) return cached;

        var setting = await _db.SystemSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting == null) return null;

        var value = setting.IsEncrypted && !string.IsNullOrEmpty(setting.Value)
            ? _encryption.Decrypt(setting.Value)
            : setting.Value;

        _cache.Set(cacheKey, value, CacheDuration);
        return value;
    }

    public async Task<string> GetValueAsync(string key, string defaultValue)
        => await GetValueAsync(key) ?? defaultValue;

    public async Task SetValueAsync(string key, string value)
    {
        var setting = await _db.SystemSettings.FirstOrDefaultAsync(s => s.Key == key);

        if (setting == null)
        {
            _db.SystemSettings.Add(new SystemSetting { Key = key, Value = value, Group = "General" });
        }
        else
        {
            setting.Value = setting.IsEncrypted && !string.IsNullOrEmpty(value)
                ? _encryption.Encrypt(value)
                : value;
        }

        await _db.SaveChangesAsync();
        _cache.Remove($"setting:{key}");
    }

    public async Task<Dictionary<string, string>> GetGroupAsync(string group)
    {
        var settings = await _db.SystemSettings
            .Where(s => s.Group == group)
            .ToListAsync();

        var result = new Dictionary<string, string>();
        foreach (var s in settings)
        {
            var value = s.IsEncrypted && !string.IsNullOrEmpty(s.Value)
                ? _encryption.Decrypt(s.Value)
                : s.Value;
            result[s.Key] = value;
        }
        return result;
    }

    public async Task<ZarinPalConfig> GetZarinPalConfigAsync()
    {
        var merchantId = await GetValueAsync("ZarinPal:MerchantId", "");
        var isSandbox = await GetValueAsync("ZarinPal:IsSandbox", "true");
        var description = await GetValueAsync("ZarinPal:Description", "خرید اعتبار");

        return new ZarinPalConfig
        {
            MerchantId = merchantId,
            IsSandbox = isSandbox.Equals("true", StringComparison.OrdinalIgnoreCase),
            Description = description
        };
    }
}
