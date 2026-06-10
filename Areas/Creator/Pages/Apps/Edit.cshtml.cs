using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.Creator.Pages.Apps;

public class EditModel : PageModel
{
    private readonly IAppService _apps;
    private readonly IAiProviderService _providers;
    private readonly ICreatorHelper _ch;
    private readonly IWebHostEnvironment _env;
    private readonly IEncryptionService _encryption;
    private readonly ApplicationDbContext _db;

    public EditModel(IAppService apps, IAiProviderService providers, ICreatorHelper ch,
        IWebHostEnvironment env, IEncryptionService encryption, ApplicationDbContext db)
    {
        _apps = apps; _providers = providers; _ch = ch; _env = env;
        _encryption = encryption; _db = db;
    }

    public AiApp App { get; set; } = null!;
    public bool CanEditPrompt { get; set; }
    public SelectList CategoryList { get; set; } = new(Enumerable.Empty<object>());
    public string ModelsJson { get; set; } = "[]";
    public List<AppInputField> ExistingFields { get; set; } = new();
    public string ExistingFieldsJson { get; set; } = "[]";

    [BindProperty] public EditForm Form { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int appId)
    {
        var creatorId = await _ch.GetCreatorProfileIdAsync(User);
        if (creatorId == null) return Forbid();

        var app = await _db.Apps
            .Include(a => a.Tags)
            .Include(a => a.InputFields.OrderBy(f => f.SortOrder))
            .FirstOrDefaultAsync(a => a.Id == appId && a.CreatorProfileId == creatorId.Value);
        if (app == null) return NotFound();

        App = app;
        CanEditPrompt = app.Status == AppStatus.Draft || app.Status == AppStatus.Suspended;
        ExistingFields = app.InputFields.ToList();
        ExistingFieldsJson = JsonSerializer.Serialize(ExistingFields.Select(f => new
        {
            f.Name, f.Label, Type = f.Type.ToString(), f.Placeholder, f.IsRequired, f.Options
        }));

        var cats = await _apps.GetCategoriesAsync();
        CategoryList = new SelectList(cats, "Id", "Name", app.CategoryId);

        var models = await _providers.GetAllModelsAsync();
        var modelsData = models.Select(m => new
        {
            m.Id, m.Name, m.IsDefault,
            Capabilities = JsonSerializer.Deserialize<List<string>>(m.Capabilities) ?? new()
        });
        ModelsJson = JsonSerializer.Serialize(modelsData);

        Form = new EditForm
        {
            Title = app.Title,
            ShortDescription = app.ShortDescription,
            Description = app.Description,
            CategoryId = app.CategoryId,
            CreditCost = app.CreditCost,
            OutputType = app.OutputType,
            AiModelId = app.AiModelId,
            SystemContext = app.SystemContext,
            Tags = string.Join(", ", app.Tags.Select(t => t.TagName)),
            NewPrompt = CanEditPrompt ? _encryption.Decrypt(app.EncryptedPrompt) : null
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int appId)
    {
        var creatorId = await _ch.GetCreatorProfileIdAsync(User);
        if (creatorId == null) return Forbid();

        var app = await _db.Apps
            .Include(a => a.Tags)
            .Include(a => a.InputFields)
            .FirstOrDefaultAsync(a => a.Id == appId && a.CreatorProfileId == creatorId.Value);
        if (app == null) return NotFound();
        App = app;
        CanEditPrompt = app.Status == AppStatus.Draft || app.Status == AppStatus.Suspended;

        if (!ModelState.IsValid)
        {
            ExistingFields = app.InputFields.OrderBy(f => f.SortOrder).ToList();
            ExistingFieldsJson = JsonSerializer.Serialize(ExistingFields.Select(f => new
            {
                f.Name, f.Label, Type = f.Type.ToString(), f.Placeholder, f.IsRequired, f.Options
            }));
            var cats = await _apps.GetCategoriesAsync();
            CategoryList = new SelectList(cats, "Id", "Name");
            var models = await _providers.GetAllModelsAsync();
            ModelsJson = JsonSerializer.Serialize(models.Select(m => new
            {
                m.Id, m.Name, m.IsDefault,
                Capabilities = JsonSerializer.Deserialize<List<string>>(m.Capabilities) ?? new()
            }));
            return Page();
        }

        var tags = (Form.Tags ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)).ToList();

        string? thumbnailUrl = await SaveThumbnailAsync(Form.Thumbnail);

        var result = await _apps.UpdateAppAsync(appId, creatorId.Value, new UpdateAppDto
        {
            Title = Form.Title, ShortDescription = Form.ShortDescription,
            Description = Form.Description, CategoryId = Form.CategoryId,
            CreditCost = Form.CreditCost, SystemContext = Form.SystemContext,
            OutputType = Form.OutputType,
            AiModelId = Form.AiModelId > 0 ? Form.AiModelId : null,
            NewPlainTextPrompt = CanEditPrompt ? Form.NewPrompt : null,
            Tags = tags, ThumbnailUrl = thumbnailUrl
        });

        if (!result.IsSuccess) { TempData["Error"] = result.ErrorMessage; return RedirectToPage(new { appId }); }

        // sync فیلدها
        if (!string.IsNullOrWhiteSpace(Form.FieldsJson))
        {
            try
            {
                var fieldDtos = JsonSerializer.Deserialize<List<FieldDto>>(Form.FieldsJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (fieldDtos != null)
                {
                    _db.AppInputFields.RemoveRange(app.InputFields);
                    for (int i = 0; i < fieldDtos.Count; i++)
                    {
                        var f = fieldDtos[i];
                        if (string.IsNullOrWhiteSpace(f.Name)) continue;
                        _db.AppInputFields.Add(new AppInputField
                        {
                            AppId = appId,
                            Name = f.Name.Trim().ToLower(),
                            Label = f.Label ?? f.Name,
                            Type = Enum.TryParse<FieldType>(f.Type, true, out var ft) ? ft : FieldType.Text,
                            Placeholder = f.Placeholder,
                            IsRequired = f.Required,
                            Options = f.Options,
                            SortOrder = i + 1
                        });
                    }
                    await _db.SaveChangesAsync();
                }
            }
            catch (JsonException) { /* FieldsJson malformed — skip field sync */ }
        }

        TempData["Success"] = "ابزار بروزرسانی شد.";
        return RedirectToPage("/Apps/Index", new { area = "Creator" });
    }

    private async Task<string?> SaveThumbnailAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;
        if (file.Length > 5 * 1024 * 1024) return null;
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

    public class EditForm
    {
        [Required][MaxLength(200)] public string Title { get; set; } = string.Empty;
        [Required][MaxLength(160)] public string ShortDescription { get; set; } = string.Empty;
        [Required] public string Description { get; set; } = string.Empty;
        [Range(1, int.MaxValue)] public int CategoryId { get; set; }
        [Range(1, 1000)] public int CreditCost { get; set; } = 1;
        public OutputType OutputType { get; set; } = OutputType.Text;
        [Range(1, int.MaxValue)] public int AiModelId { get; set; }
        public string? SystemContext { get; set; }
        public string? NewPrompt { get; set; }
        public string? Tags { get; set; }
        public IFormFile? Thumbnail { get; set; }
        public string? FieldsJson { get; set; }
    }

    public class FieldDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Label { get; set; }
        public string? Type { get; set; }
        public string? Placeholder { get; set; }
        public bool Required { get; set; } = true;
        public string? Options { get; set; }
    }
}
