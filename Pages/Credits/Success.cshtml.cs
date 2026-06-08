using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Pages.Credits;

public class SuccessModel : PageModel
{
    private readonly ICreditService _credits;
    public SuccessModel(ICreditService credits) => _credits = credits;

    public string? RefId { get; set; }
    public int CreditAdded { get; set; }
    public int NewBalance { get; set; }
    public string? InfoMessage { get; set; }

    public async Task OnGetAsync()
    {
        RefId = TempData["PaymentRefId"]?.ToString();
        CreditAdded = TempData["PaymentCredit"] is int c ? c : 0;
        InfoMessage = TempData["PaymentInfo"]?.ToString();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId != null)
            NewBalance = await _credits.GetBalanceAsync(userId);
    }
}
