using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Enums;

namespace PromptMarketPlace.Areas.Admin.Pages.Dashboard;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public decimal RevenueToday { get; set; }
    public decimal RevenueWeek { get; set; }
    public decimal RevenueMonth { get; set; }
    public int NewUsersToday { get; set; }
    public int NewUsersWeek { get; set; }
    public int ExecutionsToday { get; set; }
    public int PendingReviewCount { get; set; }

    public string UserGrowthLabels { get; set; } = "[]";
    public string UserGrowthData { get; set; } = "[]";
    public string RevenueLabels { get; set; } = "[]";
    public string RevenueData { get; set; } = "[]";

    public async Task OnGetAsync()
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var weekStart = todayStart.AddDays(-(int)todayStart.DayOfWeek);
        var monthStart = new DateTime(now.Year, now.Month, 1);

        RevenueToday = await _db.Payments
            .Where(p => p.Status == PaymentStatus.Verified && p.CreatedAt >= todayStart)
            .SumAsync(p => (decimal?)p.Amount) ?? 0;

        RevenueWeek = await _db.Payments
            .Where(p => p.Status == PaymentStatus.Verified && p.CreatedAt >= weekStart)
            .SumAsync(p => (decimal?)p.Amount) ?? 0;

        RevenueMonth = await _db.Payments
            .Where(p => p.Status == PaymentStatus.Verified && p.CreatedAt >= monthStart)
            .SumAsync(p => (decimal?)p.Amount) ?? 0;

        NewUsersToday = await _db.Users.CountAsync(u => u.CreatedAt >= todayStart);
        NewUsersWeek = await _db.Users.CountAsync(u => u.CreatedAt >= weekStart);

        ExecutionsToday = await _db.Executions.CountAsync(e => e.CreatedAt >= todayStart);

        PendingReviewCount = await _db.Apps.CountAsync(a => a.Status == AppStatus.UnderReview);

        // ۳۰ روز اخیر
        var days30 = Enumerable.Range(0, 30)
            .Select(i => todayStart.AddDays(-29 + i))
            .ToList();

        var usersByDay = await _db.Users
            .Where(u => u.CreatedAt >= days30[0])
            .GroupBy(u => u.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        var revenueByDay = await _db.Payments
            .Where(p => p.Status == PaymentStatus.Verified && p.CreatedAt >= days30[0])
            .GroupBy(p => p.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Total = g.Sum(p => p.Amount) })
            .ToListAsync();

        var userMap = usersByDay.ToDictionary(x => x.Date, x => x.Count);
        var revMap = revenueByDay.ToDictionary(x => x.Date, x => x.Total);

        var labels = days30.Select(d => $"\"{d:MM/dd}\"");
        var userData = days30.Select(d => userMap.TryGetValue(d, out var c) ? c : 0);
        var revData = days30.Select(d => revMap.TryGetValue(d, out var r) ? (long)r : 0);

        UserGrowthLabels = "[" + string.Join(",", labels) + "]";
        UserGrowthData = "[" + string.Join(",", userData) + "]";
        RevenueLabels = "[" + string.Join(",", labels) + "]";
        RevenueData = "[" + string.Join(",", revData) + "]";
    }
}
