using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Services;

public class AppService : IAppService
{
    private readonly ApplicationDbContext _db;
    private readonly IEncryptionService _encryption;
    private readonly ISlugService _slug;
    private readonly ISettingService _settings;

    public AppService(ApplicationDbContext db, IEncryptionService encryption, ISlugService slug, ISettingService settings)
    {
        _db = db;
        _encryption = encryption;
        _slug = slug;
        _settings = settings;
    }

    public async Task<PagedResult<AiApp>> GetPublishedAppsAsync(AppFilterDto filter)
    {
        var query = _db.Apps
            .Include(a => a.Category)
            .Include(a => a.Creator).ThenInclude(c => c.User)
            .Include(a => a.AiModel)
            .Where(a => a.Status == AppStatus.Active)
            .AsQueryable();

        if (filter.CategoryId.HasValue)
            query = query.Where(a => a.CategoryId == filter.CategoryId.Value);

        if (filter.OutputType.HasValue)
            query = query.Where(a => a.OutputType == filter.OutputType.Value);

        if (filter.MinCreditCost.HasValue)
            query = query.Where(a => a.CreditCost >= filter.MinCreditCost.Value);

        if (filter.MaxCreditCost.HasValue)
            query = query.Where(a => a.CreditCost <= filter.MaxCreditCost.Value);

        if (filter.MinRating.HasValue)
            query = query.Where(a => a.AverageRating >= filter.MinRating.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(a => a.Title.ToLower().Contains(search) ||
                                     a.ShortDescription.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(filter.TagName))
            query = query.Where(a => a.Tags.Any(t => t.TagName == filter.TagName));

        query = filter.SortBy switch
        {
            AppSortBy.Newest       => query.OrderByDescending(a => a.CreatedAt),
            AppSortBy.HighestRated => query.OrderByDescending(a => a.AverageRating),
            AppSortBy.LowestCost   => query.OrderBy(a => a.CreditCost),
            _                      => query.OrderByDescending(a => a.ExecutionCount)
        };

        var total = await query.CountAsync();
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return PagedResult<AiApp>.Create(items, total, filter.Page, filter.PageSize);
    }

    public async Task<AiApp?> GetAppBySlugAsync(string slug)
        => await _db.Apps
            .Include(a => a.Category)
            .Include(a => a.Creator).ThenInclude(c => c.User)
            .Include(a => a.AiModel).ThenInclude(m => m.Provider)
            .Include(a => a.InputFields.OrderBy(f => f.SortOrder))
            .Include(a => a.ShowcaseItems.OrderBy(s => s.SortOrder))
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.Slug == slug && a.Status == AppStatus.Active);

    public async Task<AiApp?> GetAppByIdAsync(int id)
        => await _db.Apps
            .Include(a => a.Category)
            .Include(a => a.Creator)
            .Include(a => a.AiModel).ThenInclude(m => m.Provider)
            .Include(a => a.InputFields.OrderBy(f => f.SortOrder))
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<PagedResult<AiApp>> GetAppsByCreatorAsync(int creatorProfileId,
        int page = 1, int pageSize = 20, AppStatus? status = null)
    {
        var query = _db.Apps
            .Include(a => a.Category)
            .Where(a => a.CreatorProfileId == creatorProfileId)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return PagedResult<AiApp>.Create(items, total, page, pageSize);
    }

    private async Task<int> GetCreditCostByOutputTypeAsync(OutputType type)
    {
        var key = type switch {
            OutputType.Image => "Pricing:ImageCreditCost",
            OutputType.Video => "Pricing:VideoCreditCost",
            OutputType.Audio => "Pricing:AudioCreditCost",
            _                => "Pricing:TextCreditCost"
        };
        var val = await _settings.GetValueAsync(key, "1");
        return int.TryParse(val, out var n) ? Math.Max(1, n) : 1;
    }

    public async Task<AiApp> CreateAppAsync(int creatorProfileId, CreateAppDto dto)
    {
        var rawSlug = _slug.GenerateSlug(dto.Title);
        var uniqueSlug = await _slug.EnsureUniqueAsync(rawSlug);

        var app = new AiApp
        {
            Slug = uniqueSlug,
            Title = dto.Title,
            ShortDescription = dto.ShortDescription,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            OutputType = dto.OutputType,
            AiModelId = dto.AiModelId,
            EncryptedPrompt = _encryption.Encrypt(dto.PlainTextPrompt),
            SystemContext = dto.SystemContext,
            ThumbnailUrl = dto.ThumbnailUrl,
            CreditCost = await GetCreditCostByOutputTypeAsync(dto.OutputType),
            CreatorProfileId = creatorProfileId,
            Status = AppStatus.Draft
        };

        foreach (var tag in dto.Tags.Distinct())
            app.Tags.Add(new AppTag { TagName = tag.Trim().ToLower() });

        _db.Apps.Add(app);
        await _db.SaveChangesAsync();
        return app;
    }

    public async Task<ExecutionResult> UpdateAppAsync(int appId, int creatorProfileId, UpdateAppDto dto)
    {
        var app = await _db.Apps
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.Id == appId && a.CreatorProfileId == creatorProfileId);

        if (app == null)
            return ExecutionResult.Fail("ابزار یافت نشد.");

        // پرامپت فقط در Draft یا Suspended قابل ویرایش است
        if (!string.IsNullOrWhiteSpace(dto.NewPlainTextPrompt) &&
            app.Status != AppStatus.Draft && app.Status != AppStatus.Suspended)
            return ExecutionResult.Fail("برای ویرایش پرامپت، ابتدا ابزار را به Draft برگردانید.");

        app.Title = dto.Title;
        app.ShortDescription = dto.ShortDescription;
        app.Description = dto.Description;
        app.CategoryId = dto.CategoryId;
        app.CreditCost = dto.CreditCost;
        app.SystemContext = dto.SystemContext;
        if (dto.ThumbnailUrl != null) app.ThumbnailUrl = dto.ThumbnailUrl;
        app.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(dto.NewPlainTextPrompt))
            app.EncryptedPrompt = _encryption.Encrypt(dto.NewPlainTextPrompt);

        // بروزرسانی تگ‌ها
        _db.AppTags.RemoveRange(app.Tags);
        foreach (var tag in dto.Tags.Distinct())
            app.Tags.Add(new AppTag { TagName = tag.Trim().ToLower() });

        await _db.SaveChangesAsync();
        return ExecutionResult.Success(null!);
    }

    public async Task<ExecutionResult> SubmitForReviewAsync(int appId, int creatorProfileId)
    {
        var app = await _db.Apps
            .Include(a => a.InputFields)
            .Include(a => a.ShowcaseItems)
            .FirstOrDefaultAsync(a => a.Id == appId && a.CreatorProfileId == creatorProfileId);

        if (app == null)
            return ExecutionResult.Fail("ابزار یافت نشد.");

        if (app.Status != AppStatus.Draft)
            return ExecutionResult.Fail("فقط ابزارهای Draft قابل ارسال برای بررسی هستند.");

        if (!app.InputFields.Any())
            return ExecutionResult.Fail("ابزار باید حداقل یک فیلد ورودی داشته باشد.");

        if (app.ShowcaseItems.Count < 3)
            return ExecutionResult.Fail("ابزار باید حداقل ۳ نمونه خروجی داشته باشد.");

        app.Status = AppStatus.UnderReview;
        app.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return ExecutionResult.Success(null!);
    }

    public async Task<ExecutionResult> UpdateStatusAsync(int appId, AppStatus newStatus, string? adminNote = null)
    {
        var app = await _db.Apps.FindAsync(appId);
        if (app == null) return ExecutionResult.Fail("ابزار یافت نشد.");

        app.Status = newStatus;
        app.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return ExecutionResult.Success(null!);
    }

    public async Task<ExecutionResult> DeleteAppAsync(int appId, int creatorProfileId)
    {
        var app = await _db.Apps
            .FirstOrDefaultAsync(a => a.Id == appId && a.CreatorProfileId == creatorProfileId);

        if (app == null) return ExecutionResult.Fail("ابزار یافت نشد.");

        if (app.Status == AppStatus.Active)
            return ExecutionResult.Fail("ابزار فعال را نمی‌توان حذف کرد. ابتدا آن را تعلیق کنید.");

        // Soft delete: تغییر وضعیت به Suspended + پاک کردن slug
        app.Status = AppStatus.Suspended;
        app.Slug = $"deleted-{app.Id}-{DateTime.UtcNow.Ticks}";
        app.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return ExecutionResult.Success(null!);
    }

    public async Task RecalculateRatingAsync(int appId)
    {
        var reviews = await _db.Reviews
            .Where(r => r.AppId == appId)
            .ToListAsync();

        if (!reviews.Any()) return;

        var avg = reviews.Average(r => r.Rating);

        await _db.Apps
            .Where(a => a.Id == appId)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.AverageRating, avg));
    }

    public async Task<List<AppCategory>> GetCategoriesAsync()
        => await _db.Categories.OrderBy(c => c.SortOrder).ToListAsync();
}
