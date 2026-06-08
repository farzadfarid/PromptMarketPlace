using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Areas.Admin.Pages.Reviews;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public List<AppReview> Reviews { get; set; } = new();
    public int TotalCount { get; set; }

    [BindProperty(SupportsGet = true)] public int? FilterRating { get; set; }
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    private const int PageSize = 30;

    public async Task OnGetAsync()
    {
        var query = _db.Reviews
            .Include(r => r.App)
            .Include(r => r.User)
            .AsQueryable();

        if (FilterRating.HasValue)
            query = query.Where(r => r.Rating == FilterRating.Value);

        if (!string.IsNullOrEmpty(Search))
            query = query.Where(r => r.App.Title.Contains(Search) ||
                                     r.User.DisplayName.Contains(Search) ||
                                     (r.Comment != null && r.Comment.Contains(Search)));

        TotalCount = await query.CountAsync();
        Reviews = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var review = await _db.Reviews.FindAsync(id);
        if (review == null) return NotFound();

        _db.Reviews.Remove(review);
        await _db.SaveChangesAsync();

        TempData["Success"] = "نظر حذف شد.";
        return RedirectToPage(new { FilterRating, Search, PageNumber });
    }
}
