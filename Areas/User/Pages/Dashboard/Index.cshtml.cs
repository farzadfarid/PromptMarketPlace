using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.User.Pages.Dashboard;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly ICreditService _credits;

    public IndexModel(ApplicationDbContext db, ICreditService credits)
    { _db = db; _credits = credits; }

    public int CreditBalance { get; set; }
    public List<AppExecution> RecentExecutions { get; set; } = new();
    public List<UserFavorite> Favorites { get; set; } = new();
    public List<AiApp> RecommendedApps { get; set; } = new();

    public async Task OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        CreditBalance = await _credits.GetBalanceAsync(userId);

        RecentExecutions = await _db.Executions
            .Include(e => e.App)
            .Where(e => e.UserId == userId && e.Status == ExecutionStatus.Completed)
            .OrderByDescending(e => e.CreatedAt)
            .Take(5)
            .ToListAsync();

        Favorites = await _db.Favorites
            .Include(f => f.App).ThenInclude(a => a.Creator).ThenInclude(c => c.User)
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Take(4)
            .ToListAsync();

        // ØªÙˆØµÛŒÙ‡ Ø¨Ø± Ø§Ø³Ø§Ø³ Ø¯Ø³ØªÙ‡â€ŒØ¨Ù†Ø¯ÛŒâ€ŒÙ‡Ø§ÛŒ Ù¾Ø±Ú©Ø§Ø±Ø¨Ø±Ø¯
        var usedCategoryIds = await _db.Executions
            .Include(e => e.App)
            .Where(e => e.UserId == userId)
            .Select(e => e.App.CategoryId)
            .Distinct()
            .Take(3)
            .ToListAsync();

        if (usedCategoryIds.Any())
        {
            var usedAppIds = await _db.Executions
                .Where(e => e.UserId == userId)
                .Select(e => e.AppId)
                .Distinct()
                .ToListAsync();

            RecommendedApps = await _db.Apps
                .Include(a => a.Creator).ThenInclude(c => c.User)
                .Where(a => a.Status == AppStatus.Active
                            && usedCategoryIds.Contains(a.CategoryId)
                            && !usedAppIds.Contains(a.Id))
                .OrderByDescending(a => a.ExecutionCount)
                .Take(4)
                .ToListAsync();
        }

        if (!RecommendedApps.Any())
        {
            RecommendedApps = await _db.Apps
                .Include(a => a.Creator).ThenInclude(c => c.User)
                .Where(a => a.Status == AppStatus.Active)
                .OrderByDescending(a => a.ExecutionCount)
                .Take(4)
                .ToListAsync();
        }
    }
}

