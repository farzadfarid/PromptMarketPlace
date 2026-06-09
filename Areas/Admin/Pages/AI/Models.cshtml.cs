using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
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
    private readonly IHttpClientFactory _httpFactory;

    public ModelsModel(IAiProviderService providerService, ApplicationDbContext db, IHttpClientFactory httpFactory)
    {
        _providerService = providerService;
        _db = db;
        _httpFactory = httpFactory;
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

    // بارگذاری لیست مدل‌ها از سرویس‌دهنده
    public async Task<IActionResult> OnPostFetchProviderModelsAsync([FromBody] FetchModelsDto dto)
    {
        var provider = await _providerService.GetProviderByIdAsync(dto.ProviderId);
        if (provider == null)
            return new JsonResult(new { success = false, message = "سرویس‌دهنده یافت نشد." });

        var apiKey = await _providerService.GetDecryptedApiKeyAsync(dto.ProviderId);
        if (string.IsNullOrWhiteSpace(apiKey))
            return new JsonResult(new { success = false, message = "API Key تنظیم نشده است." });

        try
        {
            var client = _httpFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(15);
            var request = new HttpRequestMessage(HttpMethod.Get, $"{provider.BaseUrl.TrimEnd('/')}/models");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return new JsonResult(new { success = false, message = $"خطا از سرویس‌دهنده: {(int)response.StatusCode}" });

            var node = JsonNode.Parse(json);
            var dataArray = node?["data"]?.AsArray() ?? node?.AsArray();

            var existingIds = await _db.AiModels
                .Where(m => m.AiProviderId == dto.ProviderId)
                .Select(m => m.ModelId)
                .ToListAsync();

            var models = new List<object>();
            if (dataArray != null)
            {
                foreach (var item in dataArray)
                {
                    var id   = item?["id"]?.GetValue<string>() ?? "";
                    var name = item?["name"]?.GetValue<string>() ?? id;
                    if (!string.IsNullOrWhiteSpace(id))
                        models.Add(new { id, name, alreadyAdded = existingIds.Contains(id) });
                }
            }

            return new JsonResult(new { success = true, models, providerName = provider.Name });
        }
        catch (TaskCanceledException)
        {
            return new JsonResult(new { success = false, message = "زمان انتظار به پایان رسید." });
        }
        catch (Exception)
        {
            return new JsonResult(new { success = false, message = "خطا در دریافت مدل‌ها." });
        }
    }

    // ذخیره مدل‌های انتخاب‌شده
    public async Task<IActionResult> OnPostImportModelsAsync([FromBody] ImportModelsDto dto)
    {
        if (dto.Models == null || dto.Models.Count == 0)
            return new JsonResult(new { success = false, message = "هیچ مدلی انتخاب نشده." });

        var validCaps = (dto.Capabilities ?? new())
            .Where(c => Enum.TryParse<AiCapability>(c, out _))
            .ToList();
        var caps = JsonSerializer.Serialize(validCaps);

        int count = 0;
        foreach (var modelId in dto.Models)
        {
            var exists = await _db.AiModels.AnyAsync(m => m.AiProviderId == dto.ProviderId && m.ModelId == modelId);
            if (exists) continue;

            await _providerService.CreateModelAsync(new AiModel
            {
                AiProviderId = dto.ProviderId,
                Name         = modelId.Contains('/') ? modelId.Split('/').Last() : modelId,
                ModelId      = modelId,
                Capabilities = caps,
                IsActive     = true,
                SortOrder    = 0
            });
            count++;
        }

        return new JsonResult(new { success = true, message = $"{count} مدل با موفقیت اضافه شد." });
    }

    public record FetchModelsDto(int ProviderId);
    public record ImportModelsDto(int ProviderId, List<string> Models, List<string>? Capabilities);
}
