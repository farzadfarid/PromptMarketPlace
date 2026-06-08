using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Pages.Credits;

[Authorize]
public class PackagesModel : PageModel
{
    private readonly IPaymentService _payment;
    private readonly ICreditService _credits;

    public PackagesModel(IPaymentService payment, ICreditService credits)
    {
        _payment = payment;
        _credits = credits;
    }

    public List<CreditPackage> Packages { get; set; } = new();
    public int CurrentBalance { get; set; }

    public async Task OnGetAsync()
    {
        Packages = await _payment.GetActivePackagesAsync();
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        CurrentBalance = await _credits.GetBalanceAsync(userId);
    }

    public async Task<IActionResult> OnPostBuyAsync(int packageId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var callbackUrl = Url.Page("/Credits/Callback", null, null, Request.Scheme)!;
        var result = await _payment.InitiatePaymentAsync(userId, packageId, callbackUrl);

        if (!result.IsSuccess)
        {
            TempData["Error"] = result.ErrorMessage;
            return RedirectToPage();
        }

        return Redirect(result.RedirectUrl!);
    }
}
