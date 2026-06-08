using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.Admin.Pages.Apps;

public class ReviewModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IEncryptionService _encryption;

    public ReviewModel(ApplicationDbContext db, IEncryptionService encryption)
    {
        _db = db;
        _encryption = encryption;
    }

    public List<AppReviewItem> Apps { get; set; } = new();

    [BindProperty] public string? RejectReason { get; set; }

    public async Task OnGetAsync()
    {
        var apps = await _db.Apps
            .Include(a => a.Creator).ThenInclude(c => c.User)
            .Include(a => a.Category)
            .Include(a => a.AiModel)
            .Include(a => a.InputFields)
            .Include(a => a.ShowcaseItems)
            .Where(a => a.Status == AppStatus.UnderReview)
            .OrderBy(a => a.UpdatedAt)
            .ToListAsync();

        Apps = apps.Select(a => new AppReviewItem(a, DecryptSafe(a.EncryptedPrompt))).ToList();
    }

    public async Task<IActionResult> OnPostApproveAsync(int id, int creditCost)
    {
        var app = await _db.Apps.FindAsync(id);
        if (app == null) return NotFound();

        app.Status = AppStatus.Active;
        app.CreditCost = Math.Max(1, creditCost);
        app.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await AddAuditAsync("ApproveApp", "App", id.ToString(),
            $"ابزار '{app.Title}' تایید شد — هزینه: {app.CreditCost} اعتبار");

        TempData["Success"] = $"ابزار «{app.Title}» تایید شد — هزینه اجرا: {app.CreditCost} اعتبار.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(int id)
    {
        var app = await _db.Apps.FindAsync(id);
        if (app == null) return NotFound();

        app.Status = AppStatus.Draft;
        app.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var reason = RejectReason?.Trim() ?? "بدون دلیل";
        await AddAuditAsync("RejectApp", "App", id.ToString(), $"ابزار '{app.Title}' رد شد — {reason}");

        TempData["Warning"] = $"ابزار «{app.Title}» رد شد.";
        return RedirectToPage();
    }

    private string DecryptSafe(string encrypted)
    {
        try { return _encryption.Decrypt(encrypted); }
        catch { return "[خطا در رمزگشایی]"; }
    }

    private async Task AddAuditAsync(string action, string targetType, string targetId, string details)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        _db.AuditLogs.Add(new AdminAuditLog
        {
            AdminUserId = adminId,
            Action = action,
            TargetType = targetType,
            TargetId = targetId,
            Details = details,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        });
        await _db.SaveChangesAsync();
    }

    public record AppReviewItem(AiApp App, string DecryptedPrompt);
}
