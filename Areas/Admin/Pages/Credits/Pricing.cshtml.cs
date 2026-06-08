using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.Admin.Pages.Credits;

public class PricingModel : PageModel
{
    private readonly ISettingService _settings;
    private readonly IAiProviderService _providers;

    public PricingModel(ISettingService settings, IAiProviderService providers)
    {
        _settings = settings;
        _providers = providers;
    }

    [BindProperty] public decimal UsdToIrrRate    { get; set; }
    [BindProperty] public decimal VatPercent      { get; set; }
    [BindProperty] public decimal MarginPercent   { get; set; }
    [BindProperty] public decimal CreditValueIrr  { get; set; }
    [BindProperty] public int     AvgTextTokens   { get; set; }
    [BindProperty] public int     AvgVideoSeconds { get; set; }
    [BindProperty] public int     TextCreditCost  { get; set; }
    [BindProperty] public int     ImageCreditCost { get; set; }
    [BindProperty] public int     VideoCreditCost { get; set; }
    [BindProperty] public int     AudioCreditCost { get; set; }

    public List<ModelCostInfo> ModelCosts { get; set; } = new();

    public async Task OnGetAsync()
    {
        UsdToIrrRate    = decimal.Parse(await _settings.GetValueAsync("Pricing:UsdToIrrRate",    "900000"));
        VatPercent      = decimal.Parse(await _settings.GetValueAsync("Pricing:VatPercent",      "9"));
        MarginPercent   = decimal.Parse(await _settings.GetValueAsync("Pricing:MarginPercent",   "30"));
        CreditValueIrr  = decimal.Parse(await _settings.GetValueAsync("Pricing:CreditValueIrr", "1000"));
        AvgTextTokens   = int.Parse(await _settings.GetValueAsync("Pricing:AvgTextTokens",   "1500"));
        AvgVideoSeconds = int.Parse(await _settings.GetValueAsync("Pricing:AvgVideoSeconds", "60"));
        TextCreditCost  = int.Parse(await _settings.GetValueAsync("Pricing:TextCreditCost",  "1"));
        ImageCreditCost = int.Parse(await _settings.GetValueAsync("Pricing:ImageCreditCost", "5"));
        VideoCreditCost = int.Parse(await _settings.GetValueAsync("Pricing:VideoCreditCost", "20"));
        AudioCreditCost = int.Parse(await _settings.GetValueAsync("Pricing:AudioCreditCost", "3"));
        await LoadModelCostsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _settings.SetValueAsync("Pricing:UsdToIrrRate",    UsdToIrrRate.ToString());
        await _settings.SetValueAsync("Pricing:VatPercent",      VatPercent.ToString());
        await _settings.SetValueAsync("Pricing:MarginPercent",   MarginPercent.ToString());
        await _settings.SetValueAsync("Pricing:CreditValueIrr",  CreditValueIrr.ToString());
        await _settings.SetValueAsync("Pricing:AvgTextTokens",   AvgTextTokens.ToString());
        await _settings.SetValueAsync("Pricing:AvgVideoSeconds", AvgVideoSeconds.ToString());
        await _settings.SetValueAsync("Pricing:TextCreditCost",  TextCreditCost.ToString());
        await _settings.SetValueAsync("Pricing:ImageCreditCost", ImageCreditCost.ToString());
        await _settings.SetValueAsync("Pricing:VideoCreditCost", VideoCreditCost.ToString());
        await _settings.SetValueAsync("Pricing:AudioCreditCost", AudioCreditCost.ToString());

        TempData["Success"] = "تعرفه‌ها با موفقیت ذخیره شدند.";
        return RedirectToPage();
    }

    private async Task LoadModelCostsAsync()
    {
        var models = await _providers.GetAllModelsAsync();
        ModelCosts = models.Where(m => m.IsActive).Select(m => new ModelCostInfo
        {
            Name               = m.Name,
            CostPer1KTokens    = m.CostPer1KTokens    != null ? (decimal)m.CostPer1KTokens    : null,
            CostPerImage       = m.CostPerImage       != null ? (decimal)m.CostPerImage       : null,
            CostPerSecondVideo = m.CostPerSecondVideo != null ? (decimal)m.CostPerSecondVideo : null,
            EstimatedTextCredit = m.CostPer1KTokens.HasValue
                ? (int)Math.Ceiling((decimal)AvgTextTokens / 1000m
                    * (decimal)m.CostPer1KTokens.Value
                    * UsdToIrrRate
                    * (1 + VatPercent / 100)
                    * (1 + MarginPercent / 100)
                    / CreditValueIrr)
                : null,
            EstimatedImageCredit = m.CostPerImage.HasValue
                ? (int)Math.Ceiling((decimal)m.CostPerImage.Value
                    * UsdToIrrRate
                    * (1 + VatPercent / 100)
                    * (1 + MarginPercent / 100)
                    / CreditValueIrr)
                : null,
            EstimatedVideoCredit = m.CostPerSecondVideo.HasValue
                ? (int)Math.Ceiling((decimal)AvgVideoSeconds
                    * (decimal)m.CostPerSecondVideo.Value
                    * UsdToIrrRate
                    * (1 + VatPercent / 100)
                    * (1 + MarginPercent / 100)
                    / CreditValueIrr)
                : null,
        }).ToList();
    }

    public class ModelCostInfo
    {
        public string Name                { get; set; } = "";
        public decimal? CostPer1KTokens    { get; set; }
        public decimal? CostPerImage       { get; set; }
        public decimal? CostPerSecondVideo { get; set; }
        public int? EstimatedTextCredit   { get; set; }
        public int? EstimatedImageCredit  { get; set; }
        public int? EstimatedVideoCredit  { get; set; }
    }
}
