using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.Admin.Pages.Settings;

public class IndexModel : PageModel
{
    private readonly ISettingService _settings;
    private readonly IHttpClientFactory _httpFactory;
    public IndexModel(ISettingService settings, IHttpClientFactory httpFactory)
    { _settings = settings; _httpFactory = httpFactory; }

    [BindProperty] public SettingsForm Form { get; set; } = new();
    public string? TestResult { get; set; }

    public async Task OnGetAsync()
    {
        Form.MerchantId = await _settings.GetValueAsync("ZarinPal:MerchantId", "");
        Form.IsSandbox = (await _settings.GetValueAsync("ZarinPal:IsSandbox", "true")) == "true";
        Form.Description = await _settings.GetValueAsync("ZarinPal:Description", "خرید اعتبار");
        Form.SiteName = await _settings.GetValueAsync("General:SiteName", "پرامپت مارکت");
        Form.SupportEmail = await _settings.GetValueAsync("General:SupportEmail", "");
        Form.MinWithdrawal = await _settings.GetValueAsync("Withdrawal:MinimumAmount", "500000");
        Form.AutoApproveCreator = (await _settings.GetValueAsync("Registration:AutoApproveCreator", "true")) == "true";
        Form.PlatformCommission = await _settings.GetValueAsync("Commission:PlatformPercent", "30");
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        await _settings.SetValueAsync("ZarinPal:MerchantId", Form.MerchantId);
        await _settings.SetValueAsync("ZarinPal:IsSandbox", Form.IsSandbox ? "true" : "false");
        await _settings.SetValueAsync("ZarinPal:Description", Form.Description);
        await _settings.SetValueAsync("General:SiteName", Form.SiteName);
        await _settings.SetValueAsync("General:SupportEmail", Form.SupportEmail);
        await _settings.SetValueAsync("Withdrawal:MinimumAmount", Form.MinWithdrawal);
        await _settings.SetValueAsync("Registration:AutoApproveCreator", Form.AutoApproveCreator ? "true" : "false");
        await _settings.SetValueAsync("Commission:PlatformPercent", Form.PlatformCommission);

        TempData["Success"] = "تنظیمات ذخیره شد.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostTestZarinpalAsync()
    {
        var config = await _settings.GetZarinPalConfigAsync();
        if (string.IsNullOrWhiteSpace(config.MerchantId))
        {
            TempData["TestResult"] = "❌ MerchantId تنظیم نشده است.";
            return RedirectToPage();
        }

        try
        {
            var client = _httpFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            var response = await client.GetAsync(config.IsSandbox
                ? "https://sandbox.zarinpal.com/pg/v4/payment/request.json"
                : "https://payment.zarinpal.com/pg/v4/payment/request.json");

            TempData["TestResult"] = response.IsSuccessStatusCode || (int)response.StatusCode < 500
                ? "✅ اتصال به زرین‌پال برقرار است."
                : $"⚠️ پاسخ: {response.StatusCode}";
        }
        catch (Exception ex)
        {
            TempData["TestResult"] = $"❌ خطا: {ex.Message}";
        }

        return RedirectToPage();
    }

    public class SettingsForm
    {
        public string MerchantId { get; set; } = string.Empty;
        public bool IsSandbox { get; set; } = true;
        public string Description { get; set; } = "خرید اعتبار";
        public string SiteName { get; set; } = string.Empty;
        public string SupportEmail { get; set; } = string.Empty;
        public string MinWithdrawal { get; set; } = "500000";
        public bool AutoApproveCreator { get; set; } = true;
        public string PlatformCommission { get; set; } = "30";
    }
}
