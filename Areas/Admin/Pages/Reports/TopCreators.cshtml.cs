using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;

namespace PromptMarketPlace.Areas.Admin.Pages.Reports;

public class TopCreatorsModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public TopCreatorsModel(ApplicationDbContext db) => _db = db;

    public List<CreatorRow> Creators { get; set; } = new();

    public async Task OnGetAsync()
    {
        Creators = await _db.CreatorProfiles
            .Include(c => c.User).ThenInclude(u => u.Wallet)
            .Where(c => c.User.IsActive)
            .OrderByDescending(c => c.User.Wallet != null ? c.User.Wallet.TotalEarned : 0)
            .Take(50)
            .Select(c => new CreatorRow(
                c.User.DisplayName,
                c.User.Email ?? "",
                c.User.Wallet != null ? c.User.Wallet.TotalEarned : 0,
                c.User.Wallet != null ? c.User.Wallet.EarningBalance : 0,
                c.Apps.Count(a => a.Status == Models.Enums.AppStatus.Active),
                c.JoinedAt,
                c.CommissionPercent
            ))
            .ToListAsync();
    }

    public record CreatorRow(string DisplayName, string Email, decimal TotalEarned,
        decimal EarningBalance, int ActiveApps, DateTime JoinedAt, decimal CommissionPercent);
}
