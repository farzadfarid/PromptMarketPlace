using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.Admin.Pages.Apps;

[Authorize(Roles = "Admin")]
public class DetailModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IEncryptionService _encryption;
    private readonly IAiProviderService _providers;
    private readonly IMessageService _msg;
    private readonly IReviewService _reviews;

    public SelectList? ModelSelectList { get; set; }
    public SelectList? CategorySelectList { get; set; }

    public DetailModel(ApplicationDbContext db, IEncryptionService encryption, IAiProviderService providers,
        IMessageService msg, IReviewService reviews)
    {
        _providers = providers;
        _db = db;
        _encryption = encryption;
        _msg = msg;
        _reviews = reviews;
    }

    public AiApp App { get; set; } = null!;
    public string DecryptedPrompt { get; set; } = string.Empty;
    public int PromptCharCount => DecryptedPrompt.Length;
    public int SystemContextCharCount => App?.SystemContext?.Length ?? 0;

    public List<AppExecution> RecentExecutions { get; set; } = new();
    public int ExecutionTotalCount { get; set; }
    [BindProperty(SupportsGet = true)] public ExecutionStatus? FilterStatus { get; set; }
    [BindProperty(SupportsGet = true)] public string? ExecSearch { get; set; }
    [BindProperty(SupportsGet = true)] public int ExecPage { get; set; } = 1;
    private const int ExecPageSize = 15;

    // Token stats (#3 and #4)
    public int? AvgTokensAll { get; set; }
    public int? AvgTokens30d { get; set; }
    public bool IsUnprofitable { get; set; }
    private const int TokensPerCredit = 1000;

    public List<AppReview> Reviews { get; set; } = new();
    public int ReviewTotalCount { get; set; }
    [BindProperty(SupportsGet = true)] public int? FilterRating { get; set; }
    [BindProperty(SupportsGet = true)] public int ReviewPage { get; set; } = 1;
    private const int ReviewPageSize = 10;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var app = await _db.Apps
            .Include(a => a.Creator).ThenInclude(c => c.User)
            .Include(a => a.Category)
            .Include(a => a.AiModel)
            .Include(a => a.InputFields)
            .Include(a => a.Tags)
            .Include(a => a.ShowcaseItems.OrderBy(s => s.SortOrder))
            .FirstOrDefaultAsync(a => a.Id == id);

        if (app == null) return NotFound();
        App = app;

        try { DecryptedPrompt = _encryption.Decrypt(app.EncryptedPrompt); }
        catch { DecryptedPrompt = "[خطا در رمزگشایی]"; }

        var allModels = await _providers.GetAllModelsAsync();
        ModelSelectList = new SelectList(
            allModels.Select(m => new { m.Id, Display = $"{m.Name} ({m.Provider?.Name})" }),
            "Id", "Display", app.AiModelId);

        var categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
        CategorySelectList = new SelectList(categories, "Id", "Name", app.CategoryId);

        var execQuery = _db.Executions
            .Include(e => e.User)
            .Where(e => e.AppId == id)
            .AsQueryable();

        if (FilterStatus.HasValue)
            execQuery = execQuery.Where(e => e.Status == FilterStatus.Value);

        if (!string.IsNullOrWhiteSpace(ExecSearch))
            execQuery = execQuery.Where(e => e.User.DisplayName.Contains(ExecSearch));

        ExecutionTotalCount = await execQuery.CountAsync();
        RecentExecutions = await execQuery
            .OrderByDescending(e => e.CreatedAt)
            .Skip((ExecPage - 1) * ExecPageSize)
            .Take(ExecPageSize)
            .ToListAsync();

        var reviewQuery = _db.Reviews
            .Include(r => r.User)
            .Where(r => r.AppId == id)
            .AsQueryable();

        if (FilterRating.HasValue)
            reviewQuery = reviewQuery.Where(r => r.Rating == FilterRating.Value);

        ReviewTotalCount = await reviewQuery.CountAsync();
        Reviews = await reviewQuery
            .OrderByDescending(r => r.CreatedAt)
            .Skip((ReviewPage - 1) * ReviewPageSize)
            .Take(ReviewPageSize)
            .ToListAsync();

        var tokenData = await _db.Executions
            .Where(e => e.AppId == id && e.Status == ExecutionStatus.Completed
                     && e.TokensUsed.HasValue && e.TokensUsed.Value > 0)
            .Select(e => new { e.TokensUsed, e.CreatedAt })
            .ToListAsync();

        if (tokenData.Any())
        {
            AvgTokensAll = (int)tokenData.Average(e => (double)e.TokensUsed!.Value);
            var cutoff = DateTime.UtcNow.AddDays(-30);
            var recent = tokenData.Where(e => e.CreatedAt >= cutoff).ToList();
            if (recent.Any())
            {
                AvgTokens30d = (int)recent.Average(e => (double)e.TokensUsed!.Value);
                IsUnprofitable = AvgTokens30d > app.CreditCost * TokensPerCredit;
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostToggleStatusAsync(int id, string targetStatus)
    {
        var app = await _db.Apps.FindAsync(id);
        if (app == null) return NotFound();

        if (!Enum.TryParse<AppStatus>(targetStatus, out var status)) return BadRequest();
        app.Status = status;
        app.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        _db.AuditLogs.Add(new AdminAuditLog
        {
            AdminUserId = adminId,
            Action = "ChangeAppStatus",
            TargetType = "App",
            TargetId = id.ToString(),
            Details = $"وضعیت '{app.Title}' به {status} تغییر یافت",
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        });
        await _db.SaveChangesAsync();

        TempData["Success"] = $"وضعیت ابزار به {status} تغییر یافت.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostSetCreditCostAsync(int id, int creditCost)
    {
        var app = await _db.Apps.FindAsync(id);
        if (app == null) return NotFound();

        app.CreditCost = Math.Max(1, creditCost);
        app.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"هزینه اجرا به {app.CreditCost} اعتبار تغییر یافت.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostChangeCategoryAsync(int id, int newCategoryId)
    {
        var app = await _db.Apps.FindAsync(id);
        if (app == null) return NotFound();

        app.CategoryId = newCategoryId;
        app.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["Success"] = "دسته‌بندی ابزار تغییر یافت.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostChangeModelAsync(int id, int newModelId)
    {
        var app = await _db.Apps.Include(a => a.AiModel).FirstOrDefaultAsync(a => a.Id == id);
        if (app == null) return NotFound();

        var model = await _db.AiModels.Include(m => m.Provider).FirstOrDefaultAsync(m => m.Id == newModelId);
        if (model == null)
        {
            TempData["Error"] = "مدل انتخابی یافت نشد.";
            return RedirectToPage(new { id });
        }

        var oldName = app.AiModel?.Name ?? "?";
        app.AiModelId = newModelId;
        app.UpdatedAt = DateTime.UtcNow;

        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        _db.AuditLogs.Add(new AdminAuditLog
        {
            AdminUserId = adminId,
            Action = "ChangeAppModel",
            TargetType = "App",
            TargetId = id.ToString(),
            Details = $"مدل ابزار '{app.Title}' از '{oldName}' به '{model.Name}' تغییر یافت",
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = $"مدل ابزار به «{model.Name}» تغییر یافت.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostApproveReviewAsync(int id, int reviewId)
    {
        await _reviews.ApproveAsync(reviewId);
        TempData["Success"] = "نظر تایید شد.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteReviewAsync(int id, int reviewId)
    {
        await _reviews.RejectAsync(reviewId);
        TempData["Success"] = "نظر حذف شد.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostSendMessageAsync(int id, string subject, string message)
    {
        var app = await _db.Apps.Include(a => a.Creator).FirstOrDefaultAsync(a => a.Id == id);
        if (app == null) return NotFound();
        if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(message))
        {
            TempData["Error"] = "موضوع و پیام نمی‌توانند خالی باشند.";
            return RedirectToPage(new { id });
        }
        var thread = await _msg.StartThreadAsync(app.CreatorProfileId, subject.Trim(), appId: id);
        await _msg.SendAsync(thread.Id, isFromAdmin: true, content: message.Trim());
        TempData["Success"] = "پیام برای سازنده ارسال شد.";
        return RedirectToPage("/Messages/Thread", new { area = "Admin", id = thread.Id });
    }
}
