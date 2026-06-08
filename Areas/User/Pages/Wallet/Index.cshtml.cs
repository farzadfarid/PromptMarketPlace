using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.User.Pages.Wallet;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ICreditService _credits;
    private readonly IPaymentService _payment;

    public IndexModel(ICreditService credits, IPaymentService payment)
    { _credits = credits; _payment = payment; }

    public int CreditBalance { get; set; }
    public List<WalletTransaction> RecentTransactions { get; set; } = new();
    public List<Payment> RecentPayments { get; set; } = new();

    public async Task OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        CreditBalance = await _credits.GetBalanceAsync(userId);
        RecentTransactions = await _credits.GetTransactionHistoryAsync(userId, pageSize: 10);
        RecentPayments = await _payment.GetPaymentHistoryAsync(userId, pageSize: 5);
    }
}

