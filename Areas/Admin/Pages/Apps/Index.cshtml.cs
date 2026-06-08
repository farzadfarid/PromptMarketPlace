using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;

namespace PromptMarketPlace.Areas.Admin.Pages.Apps;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public List<AiApp> Apps { get; set; } = new();
    public List<AppCategory> Categories { get; set; } = new();
    public int TotalCount { get; set; }

    [BindProperty(SupportsGet = true)] public string? FilterStatus { get; set; }
    [BindProperty(SupportsGet = true)] public int? FilterCategory { get; set; }
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    private const int PageSize = 30;

    public async Task OnGetAsync()
    {
        Categories = await _db.Categories.OrderBy(c => c.SortOrder).ToListAsync();

        var query = _db.Apps
            .Include(a => a.Creator).ThenInclude(c => c.User)
            .Include(a => a.Category)
            .AsQueryable();

        if (!string.IsNullOrEmpty(Search))
            query = query.Where(a => a.Title.Contains(Search));

        if (!string.IsNullOrEmpty(FilterStatus) && Enum.TryParse<AppStatus>(FilterStatus, out var status))
            query = query.Where(a => a.Status == status);

        if (FilterCategory.HasValue)
            query = query.Where(a => a.CategoryId == FilterCategory.Value);

        TotalCount = await query.CountAsync();
        Apps = await query
            .OrderByDescending(a => a.UpdatedAt)
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostToggleStatusAsync(int id, string targetStatus)
    {
        var app = await _db.Apps.FindAsync(id);
        if (app == null) return NotFound();

        if (!Enum.TryParse<AppStatus>(targetStatus, out var status)) return BadRequest();
        var oldStatus = app.Status;
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
            Details = $"وضعیت '{app.Title}' از {oldStatus} به {status} تغییر یافت",
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        });
        await _db.SaveChangesAsync();

        TempData["Success"] = $"وضعیت ابزار «{app.Title}» به {status} تغییر یافت.";
        return RedirectToPage(new { FilterStatus, FilterCategory, Search, PageNumber });
    }
}
