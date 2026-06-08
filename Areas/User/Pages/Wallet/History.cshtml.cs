using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.User.Pages.Wallet;

[Authorize]
public class HistoryModel : PageModel
{
    private readonly ICreditService _credits;
    public HistoryModel(ICreditService credits) => _credits = credits;

    public List<WalletTransaction> Transactions { get; set; } = new();
    [BindProperty(SupportsGet = true)] public TransactionType? FilterType { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;

    public async Task OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        Transactions = await _credits.GetTransactionHistoryAsync(userId, PageNumber, 30);

        if (FilterType.HasValue)
            Transactions = Transactions.Where(t => t.Type == FilterType.Value).ToList();
    }
}

