using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;

namespace PromptMarketPlace.Areas.Creator.Pages.Dashboard;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public CreatorProfile? Creator { get; set; }
    public UserWallet? Wallet { get; set; }
    public List<AiApp> TopApps { get; set; } = new();
    public List<AppReview> LatestReviews { get; set; } = new();
    public int WeekExecutions { get; set; }
    public decimal MonthEarning { get; set; }
    public decimal LastMonthEarning { get; set; }
    public string ChartLabels { get; set; } = "[]";
    public string ChartData { get; set; } = "[]";

    public async Task OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        Creator = await _db.CreatorProfiles
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (Creator == null) return;

        Wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);

        var appIds = await _db.Apps
            .Where(a => a.CreatorProfileId == Creator.Id)
            .Select(a => a.Id)
            .ToListAsync();

        TopApps = await _db.Apps
            .Where(a => a.CreatorProfileId == Creator.Id)
            .OrderByDescending(a => a.ExecutionCount)
            .Take(5)
            .ToListAsync();

        LatestReviews = await _db.Reviews
            .Include(r => r.User)
            .Include(r => r.App)
            .Where(r => appIds.Contains(r.AppId))
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .ToListAsync();

        var weekStart = DateTime.UtcNow.AddDays(-7);
        WeekExecutions = await _db.Executions
            .CountAsync(e => appIds.Contains(e.AppId) && e.CreatedAt >= weekStart
                             && e.Status == ExecutionStatus.Completed);

        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastMonthStart = monthStart.AddMonths(-1);

        MonthEarning = await _db.WalletTransactions
            .Where(t => t.UserId == userId && t.Type == TransactionType.Earn
                        && t.CreatedAt >= monthStart)
            .SumAsync(t => t.MoneyAmount ?? 0);

        LastMonthEarning = await _db.WalletTransactions
            .Where(t => t.UserId == userId && t.Type == TransactionType.Earn
                        && t.CreatedAt >= lastMonthStart && t.CreatedAt < monthStart)
            .SumAsync(t => t.MoneyAmount ?? 0);

        // داده‌های نمودار ۳۰ روز اخیر
        var since = DateTime.UtcNow.Date.AddDays(-29);
        var execsByDay = await _db.Executions
            .Where(e => appIds.Contains(e.AppId) && e.CreatedAt >= since
                        && e.Status == ExecutionStatus.Completed)
            .GroupBy(e => e.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        var labels = new List<string>();
        var data = new List<int>();
        for (int i = 29; i >= 0; i--)
        {
            var d = DateTime.UtcNow.Date.AddDays(-i);
            labels.Add(d.ToString("MM/dd"));
            data.Add(execsByDay.FirstOrDefault(x => x.Date == d)?.Count ?? 0);
        }

        ChartLabels = JsonSerializer.Serialize(labels);
        ChartData = JsonSerializer.Serialize(data);
    }
}
