using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Services.Interfaces;
using PromptMarketPlace.ViewModels.Admin;

namespace PromptMarketPlace.Areas.Admin.Pages.AI;

public class ProvidersModel : PageModel
{
    private readonly IAiProviderService _providerService;

    public ProvidersModel(IAiProviderService providerService)
        => _providerService = providerService;

    public List<AiProvider> Providers { get; set; } = new();

    [BindProperty]
    public AiProviderFormViewModel Form { get; set; } = new();

    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
        => Providers = await _providerService.GetAllProvidersAsync();

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (!ModelState.IsValid)
        {
            Providers = await _providerService.GetAllProvidersAsync();
            return Page();
        }

        if (Form.Id == 0)
            await _providerService.CreateProviderAsync(Form.Name, Form.BaseUrl, Form.ApiKey, Form.Description);
        else
            await _providerService.UpdateProviderAsync(Form.Id, Form.Name, Form.BaseUrl, Form.ApiKey, Form.Description);

        TempData["Success"] = Form.Id == 0 ? "سرویس‌دهنده با موفقیت افزوده شد." : "سرویس‌دهنده بروزرسانی شد.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        await _providerService.ToggleProviderActiveAsync(id);
        return RedirectToPage();
    }
}
