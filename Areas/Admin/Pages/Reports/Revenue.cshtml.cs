using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Enums;

namespace PromptMarketPlace.Areas.Admin.Pages.Reports;

public class RevenueModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public RevenueModel(ApplicationDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)] public DateTime? From { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? To { get; set; }

    public decimal TotalRevenue { get; set; }
    public decimal PurchaseRevenue { get; set; }
    public decimal CommissionRevenue { get; set; }
    public List<DailyRevenue> ByDay { get; set; } = new();
    public string ChartLabels { get; set; } = "[]";
    public string ChartData { get; set; } = "[]";

    public async Task OnGetAsync()
    {
        var from = From ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var to = (To ?? DateTime.UtcNow.Date).AddDays(1);

        // درآمد از خرید اعتبار
        var payments = await _db.Payments
            .Where(p => p.Status == PaymentStatus.Verified &&
                        p.CreatedAt >= from && p.CreatedAt < to)
            .GroupBy(p => p.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Total = g.Sum(p => p.Amount) })
            .ToListAsync();

        // درآمد پلتفرم از کمیسیون اجراها (سهم پلتفرم = مصرف - درآمد سازنده)
        var execSpend = await _db.WalletTransactions
            .Where(t => t.Type == TransactionType.Spend &&
                        t.CreatedAt >= from && t.CreatedAt < to)
            .SumAsync(t => (decimal?)t.CreditAmount) ?? 0;

        var creatorEarn = await _db.WalletTransactions
            .Where(t => t.Type == TransactionType.Earn &&
                        t.CreatedAt >= from && t.CreatedAt < to)
            .SumAsync(t => (decimal?)t.CreditAmount) ?? 0;

        PurchaseRevenue = payments.Sum(p => p.Total);
        CommissionRevenue = execSpend - creatorEarn;
        TotalRevenue = PurchaseRevenue + CommissionRevenue;

        var payMap = payments.ToDictionary(p => p.Date, p => p.Total);

        var days = Enumerable.Range(0, (int)(to - from).TotalDays)
            .Select(i => from.AddDays(i))
            .ToList();

        ByDay = days.Select(d => new DailyRevenue(d, payMap.TryGetValue(d, out var v) ? v : 0)).ToList();

        ChartLabels = "[" + string.Join(",", days.Select(d => $"\"{d:MM/dd}\"")) + "]";
        ChartData = "[" + string.Join(",", ByDay.Select(d => (long)d.Amount)) + "]";
    }

    public record DailyRevenue(DateTime Date, decimal Amount);
}
