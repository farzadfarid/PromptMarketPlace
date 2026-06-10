using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.Admin.Pages.Reviews;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IReviewService _reviews;

    public IndexModel(ApplicationDbContext db, IReviewService reviews)
    {
        _db = db;
        _reviews = reviews;
    }

    public List<AppReview> Reviews { get; set; } = new();
    public int TotalCount { get; set; }
    public int PendingCount { get; set; }

    [BindProperty(SupportsGet = true)] public int? FilterRating { get; set; }
    [BindProperty(SupportsGet = true)] public string? FilterStatus { get; set; }
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

        if (FilterStatus == "pending")
            query = query.Where(r => !r.IsApproved);
        else if (FilterStatus == "approved")
            query = query.Where(r => r.IsApproved);

        if (!string.IsNullOrEmpty(Search))
            query = query.Where(r => r.App.Title.Contains(Search) ||
                                     r.User.DisplayName.Contains(Search) ||
                                     (r.Comment != null && r.Comment.Contains(Search)));

        PendingCount = await _db.Reviews.CountAsync(r => !r.IsApproved);
        TotalCount = await query.CountAsync();
        Reviews = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostApproveAsync(int id)
    {
        var result = await _reviews.ApproveAsync(id);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess ? "نظر تایید شد." : result.ErrorMessage;
        return RedirectToPage(new { FilterRating, FilterStatus, Search, PageNumber });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var result = await _reviews.RejectAsync(id);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess ? "نظر حذف شد." : result.ErrorMessage;
        return RedirectToPage(new { FilterRating, FilterStatus, Search, PageNumber });
    }
}
