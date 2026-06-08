using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.Admin.Pages.Payments;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly ICreditService _credits;
    public IndexModel(ApplicationDbContext db, ICreditService credits)
    { _db = db; _credits = credits; }

    public List<Payment> Payments { get; set; } = new();
    public int TotalCount { get; set; }

    [BindProperty(SupportsGet = true)] public string? SearchUser { get; set; }
    [BindProperty(SupportsGet = true)] public string? FilterStatus { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    private const int PageSize = 30;

    public async Task OnGetAsync()
    {
        var query = _db.Payments
            .Include(p => p.User)
            .Include(p => p.Package)
            .AsQueryable();

        if (!string.IsNullOrEmpty(SearchUser))
            query = query.Where(p => p.User.Email!.Contains(SearchUser) ||
                                     p.User.DisplayName.Contains(SearchUser));

        if (!string.IsNullOrEmpty(FilterStatus) && Enum.TryParse<PaymentStatus>(FilterStatus, out var status))
            query = query.Where(p => p.Status == status);

        TotalCount = await query.CountAsync();
        Payments = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostRefundAsync(long id)
    {
        var payment = await _db.Payments.FindAsync(id);
        if (payment == null || payment.Status != PaymentStatus.Verified)
        {
            TempData["Error"] = "این پرداخت قابل بازگشت نیست.";
            return RedirectToPage();
        }

        await _credits.AddCreditsAsync(payment.UserId, payment.CreditAmount,
            $"بازگشت اعتبار توسط ادمین — پرداخت #{id}",
            id.ToString(), TransactionType.Refund);

        payment.Status = PaymentStatus.Refunded;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"{payment.CreditAmount} اعتبار به کاربر بازگشت داده شد.";
        return RedirectToPage();
    }
}
