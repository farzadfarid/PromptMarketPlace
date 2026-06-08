using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.Admin.Pages.Categories;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly ISlugService _slugs;

    public IndexModel(ApplicationDbContext db, ISlugService slugs)
    {
        _db = db;
        _slugs = slugs;
    }

    public List<AppCategory> Categories { get; set; } = new();

    [BindProperty] public int EditId { get; set; }
    [BindProperty] public string Name { get; set; } = string.Empty;
    [BindProperty] public string? IconClass { get; set; }
    [BindProperty] public string? Description { get; set; }

    public async Task OnGetAsync()
    {
        Categories = await _db.Categories.OrderBy(c => c.SortOrder).ToListAsync();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            TempData["Error"] = "نام الزامی است.";
            return RedirectToPage();
        }

        if (EditId == 0)
        {
            var maxOrder = await _db.Categories.MaxAsync(c => (int?)c.SortOrder) ?? 0;
            _db.Categories.Add(new AppCategory
            {
                Name = Name.Trim(),
                Slug = _slugs.GenerateSlug(Name),
                IconClass = IconClass?.Trim(),
                Description = Description?.Trim(),
                SortOrder = maxOrder + 1
            });
        }
        else
        {
            var cat = await _db.Categories.FindAsync(EditId);
            if (cat == null) return NotFound();
            cat.Name = Name.Trim();
            cat.Slug = _slugs.GenerateSlug(Name);
            cat.IconClass = IconClass?.Trim();
            cat.Description = Description?.Trim();
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = EditId == 0 ? "دسته‌بندی اضافه شد." : "دسته‌بندی ویرایش شد.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var cat = await _db.Categories.Include(c => c.Apps).FirstOrDefaultAsync(c => c.Id == id);
        if (cat == null) return NotFound();

        if (cat.Apps.Any())
        {
            TempData["Error"] = "این دسته‌بندی دارای ابزار است و نمی‌توان آن را حذف کرد.";
            return RedirectToPage();
        }

        _db.Categories.Remove(cat);
        await _db.SaveChangesAsync();
        TempData["Success"] = "دسته‌بندی حذف شد.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMoveAsync(int id, string direction)
    {
        var categories = await _db.Categories.OrderBy(c => c.SortOrder).ToListAsync();
        var idx = categories.FindIndex(c => c.Id == id);
        if (idx < 0) return NotFound();

        var swapIdx = direction == "up" ? idx - 1 : idx + 1;
        if (swapIdx < 0 || swapIdx >= categories.Count) return RedirectToPage();

        (categories[idx].SortOrder, categories[swapIdx].SortOrder) =
            (categories[swapIdx].SortOrder, categories[idx].SortOrder);

        await _db.SaveChangesAsync();
        return RedirectToPage();
    }
}
