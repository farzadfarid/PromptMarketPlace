using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.Creator.Pages.Apps;

public class ShowcaseModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly ICreatorHelper _ch;
    private readonly IWebHostEnvironment _env;

    public ShowcaseModel(ApplicationDbContext db, ICreatorHelper ch, IWebHostEnvironment env)
    { _db = db; _ch = ch; _env = env; }

    public AiApp App { get; set; } = null!;
    public List<AppShowcaseItem> Items { get; set; } = new();
    [BindProperty] public ShowcaseForm Form { get; set; } = new();

    private async Task<AiApp?> GetAppAsync(int appId)
    {
        var cid = await _ch.GetCreatorProfileIdAsync(User);
        if (cid == null) return null;
        return await _db.Apps.Include(a => a.ShowcaseItems.OrderBy(s => s.SortOrder))
            .FirstOrDefaultAsync(a => a.Id == appId && a.CreatorProfileId == cid.Value);
    }

    public async Task<IActionResult> OnGetAsync(int appId)
    {
        var app = await GetAppAsync(appId);
        if (app == null) return NotFound();
        App = app; Items = app.ShowcaseItems.ToList();
        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync(int appId)
    {
        var app = await GetAppAsync(appId);
        if (app == null) return NotFound();
        App = app; Items = app.ShowcaseItems.ToList();

        var maxOrder = Items.Any() ? Items.Max(s => s.SortOrder) : 0;
        var item = new AppShowcaseItem { AppId = appId, OutputType = Form.OutputType, Caption = Form.Caption, SortOrder = maxOrder + 1 };

        if (Form.ImageFile != null && Form.ImageFile.Length > 0)
        {
            var dir = Path.Combine(_env.WebRootPath, "uploads", "showcase");
            Directory.CreateDirectory(dir);
            var ext = Path.GetExtension(Form.ImageFile.FileName);
            var fileName = $"{Guid.NewGuid():N}{ext}";
            await using var fs = new FileStream(Path.Combine(dir, fileName), FileMode.Create);
            await Form.ImageFile.CopyToAsync(fs);
            item.OutputUrl = $"/uploads/showcase/{fileName}";
        }
        else if ((Form.OutputType == OutputType.Video || Form.OutputType == OutputType.Audio)
                 && !string.IsNullOrWhiteSpace(Form.MediaUrl))
        {
            item.OutputUrl = Form.MediaUrl.Trim();
        }
        else if (!string.IsNullOrWhiteSpace(Form.TextOutput))
        {
            item.OutputText = Form.TextOutput;
        }
        else
        {
            TempData["Error"] = "فایل، لینک یا متن وارد کنید.";
            return RedirectToPage(new { appId });
        }

        _db.ShowcaseItems.Add(item);
        await _db.SaveChangesAsync();
        TempData["Success"] = "نمونه اضافه شد.";
        return RedirectToPage(new { appId });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int appId, int itemId)
    {
        var app = await GetAppAsync(appId);
        if (app == null) return NotFound();
        var item = app.ShowcaseItems.FirstOrDefault(s => s.Id == itemId);
        if (item != null) { _db.ShowcaseItems.Remove(item); await _db.SaveChangesAsync(); }
        return RedirectToPage(new { appId });
    }

    public class ShowcaseForm
    {
        public OutputType OutputType { get; set; } = OutputType.Text;
        public IFormFile? ImageFile { get; set; }
        public string? MediaUrl { get; set; }
        public string? TextOutput { get; set; }
        public string? Caption { get; set; }
    }
}
