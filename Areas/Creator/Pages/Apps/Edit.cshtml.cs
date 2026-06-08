using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PromptMarketPlace.Models;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.Creator.Pages.Apps;

public class EditModel : PageModel
{
    private readonly IAppService _apps;
    private readonly ICreatorHelper _ch;
    private readonly IWebHostEnvironment _env;

    public EditModel(IAppService apps, ICreatorHelper ch, IWebHostEnvironment env) { _apps = apps; _ch = ch; _env = env; }

    public AiApp App { get; set; } = null!;
    public bool CanEditPrompt { get; set; }
    public SelectList CategoryList { get; set; } = new(Enumerable.Empty<object>());

    [BindProperty] public EditForm Form { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int appId)
    {
        var creatorId = await _ch.GetCreatorProfileIdAsync(User);
        if (creatorId == null) return Forbid();

        var app = await _apps.GetAppByIdAsync(appId);
        if (app == null || app.CreatorProfileId != creatorId.Value) return NotFound();

        App = app;
        CanEditPrompt = app.Status == AppStatus.Draft || app.Status == AppStatus.Suspended;

        var cats = await _apps.GetCategoriesAsync();
        CategoryList = new SelectList(cats, "Id", "Name", app.CategoryId);

        Form = new EditForm
        {
            Title = app.Title, ShortDescription = app.ShortDescription,
            Description = app.Description, CategoryId = app.CategoryId,
            CreditCost = app.CreditCost, SystemContext = app.SystemContext,
            Tags = string.Join(", ", app.Tags.Select(t => t.TagName))
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int appId)
    {
        var creatorId = await _ch.GetCreatorProfileIdAsync(User);
        if (creatorId == null) return Forbid();

        var app = await _apps.GetAppByIdAsync(appId);
        if (app == null || app.CreatorProfileId != creatorId.Value) return NotFound();
        App = app;
        CanEditPrompt = app.Status == AppStatus.Draft || app.Status == AppStatus.Suspended;

        if (!ModelState.IsValid)
        {
            var cats = await _apps.GetCategoriesAsync();
            CategoryList = new SelectList(cats, "Id", "Name");
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
            NewPlainTextPrompt = CanEditPrompt ? Form.NewPrompt : null,
            Tags = tags,
            ThumbnailUrl = thumbnailUrl
        });

        if (!result.IsSuccess) { TempData["Error"] = result.ErrorMessage; return RedirectToPage(new { appId }); }

        TempData["Success"] = "ابزار بروزرسانی شد.";
        return RedirectToPage("/Apps/Index", new { area = "Creator" });
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

    public class EditForm
    {
        [Required][MaxLength(200)] public string Title { get; set; } = string.Empty;
        [Required][MaxLength(160)] public string ShortDescription { get; set; } = string.Empty;
        [Required] public string Description { get; set; } = string.Empty;
        [Range(1, int.MaxValue)] public int CategoryId { get; set; }
        [Range(1, 1000)] public int CreditCost { get; set; } = 1;
        public string? SystemContext { get; set; }
        public string? NewPrompt { get; set; }
        public string? Tags { get; set; }
        public IFormFile? Thumbnail { get; set; }
    }
}
