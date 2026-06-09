namespace PromptMarketPlace.Models.Domain;

public class AiProvider
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string? ApiKeyEncrypted { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // پیکربندی بررسی موجودی — قابل تنظیم توسط ادمین
    public string? BalanceUrl { get; set; }
    public string? BalanceJsonPath { get; set; }
    public string? BalanceCurrency { get; set; }

    // فعال بودن این provider برای هر نوع خروجی
    // حداکثر یک provider می‌تواند برای هر نوع فعال باشد
    public bool IsActiveForText { get; set; }
    public bool IsActiveForImage { get; set; }
    public bool IsActiveForVideo { get; set; }
    public bool IsActiveForAudio { get; set; }

    public ICollection<AiModel> Models { get; set; } = new List<AiModel>();
}
