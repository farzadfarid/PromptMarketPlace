using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Pages.Category;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IAppService _apps;
    public IndexModel(ApplicationDbContext db, IAppService apps) { _db = db; _apps = apps; }

    public AppCategory Category { get; set; } = null!;
    public PagedResult<AiApp> Result { get; set; } = new();
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        var cat = await _db.Categories.FirstOrDefaultAsync(c => c.Slug == slug);
        if (cat == null) return NotFound();
        Category = cat;

        Result = await _apps.GetPublishedAppsAsync(new AppFilterDto
        {
            CategoryId = cat.Id,
            Page = PageNumber,
            PageSize = 20
        });
        return Page();
    }
}
