using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.Creator.Pages.Apps;

public class SubmitModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IAppService _apps;
    private readonly ICreatorHelper _ch;
    private readonly INotificationService _notify;

    public SubmitModel(ApplicationDbContext db, IAppService apps, ICreatorHelper ch, INotificationService notify)
    { _db = db; _apps = apps; _ch = ch; _notify = notify; }

    public AiApp App { get; set; } = null!;
    public bool HasFields { get; set; }
    public bool HasShowcase { get; set; }
    public bool CanSubmit { get; set; }

    public async Task<IActionResult> OnGetAsync(int appId)
    {
        var cid = await _ch.GetCreatorProfileIdAsync(User);
        if (cid == null) return Forbid();

        var app = await _db.Apps
            .Include(a => a.InputFields)
            .Include(a => a.ShowcaseItems)
            .FirstOrDefaultAsync(a => a.Id == appId && a.CreatorProfileId == cid.Value);

        if (app == null) return NotFound();

        App = app;
        HasFields = app.InputFields.Any();
        HasShowcase = app.ShowcaseItems.Count >= 3;
        CanSubmit = HasFields && HasShowcase && app.Status == AppStatus.Draft;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int appId)
    {
        var cid = await _ch.GetCreatorProfileIdAsync(User);
        if (cid == null) return Forbid();

        var result = await _apps.SubmitForReviewAsync(appId, cid.Value);
        if (!result.IsSuccess) { TempData["Error"] = result.ErrorMessage; return RedirectToPage(new { appId }); }

        var app = await _db.Apps.FindAsync(appId);
        if (app != null)
            await _notify.CreateForAdminsAsync(
                $"ابزار جدید برای بررسی: {app.Title}",
                "سازنده ابزار خود را برای انتشار ارسال کرد.",
                $"/Admin/Apps/{appId}",
                "app_review");

        TempData["Success"] = "ابزار برای بررسی ارسال شد. پس از تایید ادمین فعال می‌شود.";
        return RedirectToPage("/Apps/Index", new { area = "Creator" });
    }
}
