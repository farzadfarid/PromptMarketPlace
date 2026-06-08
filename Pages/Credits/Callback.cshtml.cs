using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Pages.Credits;

public class CallbackModel : PageModel
{
    private readonly IPaymentService _payment;
    private readonly ILogger<CallbackModel> _logger;

    public CallbackModel(IPaymentService payment, ILogger<CallbackModel> logger)
    {
        _payment = payment;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(string? authority, string? status)
    {
        if (string.IsNullOrEmpty(authority) || status?.ToUpper() != "OK")
        {
            TempData["PaymentError"] = "پرداخت لغو شد یا با خطا مواجه شد.";
            return RedirectToPage("/Credits/Failed");
        }

        var result = await _payment.VerifyPaymentAsync(authority);

        if (!result.IsSuccess)
        {
            TempData["PaymentError"] = result.ErrorMessage;
            return RedirectToPage("/Credits/Failed");
        }

        if (result.AlreadyVerified)
        {
            TempData["PaymentInfo"] = "این تراکنش قبلاً پردازش شده است.";
            return RedirectToPage("/Credits/Success");
        }

        TempData["PaymentRefId"] = result.RefId;
        TempData["PaymentCredit"] = result.CreditAdded;
        return RedirectToPage("/Credits/Success");
    }
}
