using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PromptMarketPlace.Pages.Credits;

public class FailedModel : PageModel
{
    public string? ErrorMessage { get; set; }

    public void OnGet()
        => ErrorMessage = TempData["PaymentError"]?.ToString() ?? "پرداخت ناموفق بود.";
}
