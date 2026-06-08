using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;
using PromptMarketPlace.Services.Interfaces;
using PromptMarketPlace.ViewModels.Admin;

namespace PromptMarketPlace.Areas.Admin.Pages.AI;

public class ModelsModel : PageModel
{
    private readonly IAiProviderService _providerService;
    private readonly ApplicationDbContext _db;

    public ModelsModel(IAiProviderService providerService, ApplicationDbContext db)
    {
        _providerService = providerService;
        _db = db;
    }

    public List<AiModel> Models { get; set; } = new();
    public List<AiProvider> Providers { get; set; } = new();
    public SelectList ProviderSelectList { get; set; } = new(Enumerable.Empty<object>());
    public List<AiCapability> AllCapabilities { get; set; } = Enum.GetValues<AiCapability>().ToList();

    [BindProperty] public AiModelFormViewModel Form { get; set; } = new();
    [BindProperty(SupportsGet = true)] public int? FilterProviderId { get; set; }
    [BindProperty(SupportsGet = true)] public string? FilterCapability { get; set; }

    public async Task OnGetAsync()
    {
        Providers = await _providerService.GetAllProvidersAsync();
        ProviderSelectList = new SelectList(Providers, "Id", "Name");

        AiCapability? cap = null;
        if (!string.IsNullOrEmpty(FilterCapability) && Enum.TryParse<AiCapability>(FilterCapability, out var parsed))
            cap = parsed;

        Models = await _providerService.GetAllModelsAsync(FilterProviderId, cap);
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var caps = JsonSerializer.Serialize(Form.SelectedCapabilities.Select(c => c.ToString()).ToList());

        if (Form.Id == 0)
        {
            await _providerService.CreateModelAsync(new AiModel
            {
                AiProviderId = Form.AiProviderId,
                Name = Form.Name,
                ModelId = Form.ModelId,
                Description = Form.Description,
                Capabilities = caps,
                CostPer1KTokens = Form.CostPer1KTokens,
                CostPerImage = Form.CostPerImage,
                CostPerSecondVideo = Form.CostPerSecondVideo,
                MaxTokens = Form.MaxTokens,
                IsDefault = Form.IsDefault,
                SortOrder = Form.SortOrder,
                IsActive = true
            });
        }
        else
        {
            var model = await _providerService.GetModelByIdAsync(Form.Id)
                        ?? throw new KeyNotFoundException();
            model.AiProviderId = Form.AiProviderId;
            model.Name = Form.Name;
            model.ModelId = Form.ModelId;
            model.Description = Form.Description;
            model.Capabilities = caps;
            model.CostPer1KTokens = Form.CostPer1KTokens;
            model.CostPerImage = Form.CostPerImage;
            model.CostPerSecondVideo = Form.CostPerSecondVideo;
            model.MaxTokens = Form.MaxTokens;
            model.IsDefault = Form.IsDefault;
            model.SortOrder = Form.SortOrder;
            await _providerService.UpdateModelAsync(model);
        }

        TempData["Success"] = Form.Id == 0 ? "مدل با موفقیت افزوده شد." : "مدل بروزرسانی شد.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        await _providerService.ToggleModelActiveAsync(id);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            await _providerService.DeleteModelAsync(id);
            TempData["Success"] = "مدل حذف شد.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSetDefaultAsync(int id)
    {
        await _providerService.SetDefaultModelAsync(id);
        TempData["Success"] = "مدل پیش‌فرض تغییر کرد.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostApplyToAllAsync(int id)
    {
        var model = await _providerService.GetModelByIdAsync(id);
        if (model == null) return RedirectToPage();

        var count = await _db.Apps
            .Where(a => a.AiModelId != id)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.AiModelId, id));

        TempData["Success"] = $"مدل «{model.Name}» به {count} ابزار اعمال شد.";
        return RedirectToPage();
    }
}
