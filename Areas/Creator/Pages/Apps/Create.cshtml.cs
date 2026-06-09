using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PromptMarketPlace.Models;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.Creator.Pages.Apps;

public class CreateModel : PageModel
{
    private readonly IAppService _apps;
    private readonly IAiProviderService _providers;
    private readonly ICreatorHelper _creatorHelper;
    private readonly IWebHostEnvironment _env;
    private readonly ISettingService _settings;

    public CreateModel(IAppService apps, IAiProviderService providers, ICreatorHelper creatorHelper, IWebHostEnvironment env, ISettingService settings)
    {
        _apps = apps;
        _providers = providers;
        _creatorHelper = creatorHelper;
        _env = env;
        _settings = settings;
    }

    public SelectList CategoryList { get; set; } = new(Enumerable.Empty<object>());
    public string ModelsJson { get; set; } = "[]";
    public Dictionary<string, int> PricingByType { get; set; } = new();

    [BindProperty] public CreateForm Form { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadSelectListsAsync();
        await LoadPricingAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync();
            return Page();
        }

        var creatorId = await _creatorHelper.GetCreatorProfileIdAsync(User);
        if (creatorId == null) return Forbid();

        var tags = (Form.Tags ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrEmpty(t))
            .ToList();

        string? thumbnailUrl = await SaveThumbnailAsync(Form.Thumbnail);

        var app = await _apps.CreateAppAsync(creatorId.Value, new CreateAppDto
        {
            Title = Form.Title,
            ShortDescription = Form.ShortDescription,
            Description = Form.Description,
            CategoryId = Form.CategoryId,
            OutputType = Form.OutputType,
            AiModelId = Form.AiModelId,
            CreditCost = Form.CreditCost,
            PlainTextPrompt = Form.Prompt,
            SystemContext = string.IsNullOrWhiteSpace(Form.SystemContext) ? null : Form.SystemContext,
            Tags = tags,
            ThumbnailUrl = thumbnailUrl
        });

        TempData["Success"] = "ابزار ساخته شد. حالا فیلدهای ورودی را تعریف کنید.";
        return RedirectToPage("/Apps/Fields", new { area = "Creator", appId = app.Id });
    }

    private async Task<string?> SaveThumbnailAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext)) return null;

        var uploads = Path.Combine(_env.WebRootPath, "uploads", "thumbnails");
        Directory.CreateDirectory(uploads);
        var fileName = $"{Guid.NewGuid()}{ext}";
        var path = Path.Combine(uploads, fileName);
        await using var stream = System.IO.File.Create(path);
        await file.CopyToAsync(stream);
        return $"/uploads/thumbnails/{fileName}";
    }

    private async Task LoadPricingAsync()
    {
        PricingByType = new Dictionary<string, int>
        {
            ["Text"]  = int.Parse(await _settings.GetValueAsync("Pricing:TextCreditCost",  "1")),
            ["Image"] = int.Parse(await _settings.GetValueAsync("Pricing:ImageCreditCost", "5")),
            ["Video"] = int.Parse(await _settings.GetValueAsync("Pricing:VideoCreditCost", "20")),
            ["Audio"] = int.Parse(await _settings.GetValueAsync("Pricing:AudioCreditCost", "3")),
            ["Code"]  = int.Parse(await _settings.GetValueAsync("Pricing:TextCreditCost",  "1")),
            ["Form"]  = int.Parse(await _settings.GetValueAsync("Pricing:TextCreditCost",  "1")),
        };
    }

    private async Task LoadSelectListsAsync()
    {
        var categories = await _apps.GetCategoriesAsync();
        CategoryList = new SelectList(categories, "Id", "Name");

        var models = await _providers.GetAllModelsAsync();
        var modelsData = models.Select(m => new
        {
            m.Id, m.Name, m.IsDefault,
            Capabilities = JsonSerializer.Deserialize<List<string>>(m.Capabilities) ?? new()
        });
        ModelsJson = JsonSerializer.Serialize(modelsData);
    }

    public class CreateForm
    {
        [Required(ErrorMessage = "عنوان الزامی است")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "توضیح کوتاه الزامی است")]
        [MaxLength(160)]
        public string ShortDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "توضیح کامل الزامی است")]
        public string Description { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "دسته‌بندی الزامی است")]
        public int CategoryId { get; set; }

        public OutputType OutputType { get; set; } = OutputType.Text;

        [Range(1, int.MaxValue, ErrorMessage = "مدل AI الزامی است")]
        public int AiModelId { get; set; }

        [Range(1, 1000, ErrorMessage = "هزینه باید بین ۱ تا ۱۰۰۰ باشد")]
        public int CreditCost { get; set; } = 1;

        [Required(ErrorMessage = "پرامپت الزامی است")]
        public string Prompt { get; set; } = string.Empty;

        public string? SystemContext { get; set; }
        public string? Tags { get; set; }
        public IFormFile? Thumbnail { get; set; }
    }
}
