using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Services;

public class SlugService : ISlugService
{
    private readonly ApplicationDbContext _db;
    public SlugService(ApplicationDbContext db) => _db = db;

    public string GenerateSlug(string title)
    {
        if (string.IsNullOrWhiteSpace(title)) return "app";

        var slug = title.Trim().ToLowerInvariant();

        // فارسی/عربی → نگه می‌داریم ولی space را به - تبدیل می‌کنیم
        slug = Regex.Replace(slug, @"\s+", "-");

        // حذف کاراکترهای غیرمجاز (فقط a-z, 0-9, -, و Unicode letters مجاز)
        slug = Regex.Replace(slug, @"[^\w؀-ۿ-]", "");

        // حذف - های تکراری
        slug = Regex.Replace(slug, @"-{2,}", "-");
        slug = slug.Trim('-');

        if (string.IsNullOrEmpty(slug)) slug = "app";

        // محدود کردن طول
        if (slug.Length > 100)
            slug = slug[..100].TrimEnd('-');

        return slug;
    }

    public async Task<string> EnsureUniqueAsync(string slug, int? excludeAppId = null)
    {
        var query = _db.Apps.AsQueryable();
        if (excludeAppId.HasValue)
            query = query.Where(a => a.Id != excludeAppId.Value);

        var existing = await query
            .Where(a => a.Slug == slug || a.Slug.StartsWith(slug + "-"))
            .Select(a => a.Slug)
            .ToListAsync();

        if (!existing.Contains(slug)) return slug;

        var counter = 2;
        while (existing.Contains($"{slug}-{counter}"))
            counter++;

        return $"{slug}-{counter}";
    }
}
