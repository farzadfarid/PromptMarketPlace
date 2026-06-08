using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.Creator.Pages.Apps;

public class FieldsModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly ICreatorHelper _ch;
    public FieldsModel(ApplicationDbContext db, ICreatorHelper ch) { _db = db; _ch = ch; }

    public AiApp App { get; set; } = null!;
    public List<AppInputField> Fields { get; set; } = new();
    [BindProperty] public FieldForm Form { get; set; } = new();

    private async Task<(AiApp? app, int creatorId)> LoadAsync(int appId)
    {
        var cid = await _ch.GetCreatorProfileIdAsync(User);
        if (cid == null) return (null, 0);
        var app = await _db.Apps.Include(a => a.InputFields.OrderBy(f => f.SortOrder))
            .FirstOrDefaultAsync(a => a.Id == appId && a.CreatorProfileId == cid.Value);
        return (app, cid.Value);
    }

    public async Task<IActionResult> OnGetAsync(int appId)
    {
        var (app, _) = await LoadAsync(appId);
        if (app == null) return NotFound();
        App = app; Fields = app.InputFields.ToList();
        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync(int appId)
    {
        var (app, _) = await LoadAsync(appId);
        if (app == null) return NotFound();
        App = app;

        if (!ModelState.IsValid) { Fields = app.InputFields.ToList(); return Page(); }

        var maxOrder = app.InputFields.Any() ? app.InputFields.Max(f => f.SortOrder) : 0;
        _db.AppInputFields.Add(new AppInputField
        {
            AppId = appId, Name = Form.Name.Trim().ToLower().Replace(" ", "_"),
            Label = Form.Label, Placeholder = Form.Placeholder, HelpText = Form.HelpText,
            Type = Form.Type, Options = Form.Options, IsRequired = Form.IsRequired,
            MaxLength = Form.MaxLength > 0 ? Form.MaxLength : null,
            SortOrder = maxOrder + 1
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = "فیلد اضافه شد.";
        return RedirectToPage(new { appId });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int appId, int fieldId)
    {
        var (app, _) = await LoadAsync(appId);
        if (app == null) return NotFound();
        var field = app.InputFields.FirstOrDefault(f => f.Id == fieldId);
        if (field != null) { _db.AppInputFields.Remove(field); await _db.SaveChangesAsync(); }
        return RedirectToPage(new { appId });
    }

    public async Task<IActionResult> OnPostMoveAsync(int appId, int fieldId, string dir)
    {
        var (app, _) = await LoadAsync(appId);
        if (app == null) return NotFound();
        var fields = app.InputFields.OrderBy(f => f.SortOrder).ToList();
        var idx = fields.FindIndex(f => f.Id == fieldId);
        var swap = dir == "up" ? idx - 1 : idx + 1;
        if (swap >= 0 && swap < fields.Count)
        {
            (fields[idx].SortOrder, fields[swap].SortOrder) = (fields[swap].SortOrder, fields[idx].SortOrder);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage(new { appId });
    }

    public class FieldForm
    {
        [Required(ErrorMessage = "نام متغیر الزامی است")]
        [RegularExpression(@"^[a-z0-9_]+$", ErrorMessage = "فقط حروف کوچک انگلیسی، اعداد و _")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "عنوان نمایشی الزامی است")]
        public string Label { get; set; } = string.Empty;

        public string? Placeholder { get; set; }
        public string? HelpText { get; set; }
        public FieldType Type { get; set; } = FieldType.Text;
        public string? Options { get; set; }
        public bool IsRequired { get; set; } = true;
        public int MaxLength { get; set; }
    }
}
