using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.Creator.Pages.Reviews;

public class IndexModel : PageModel
{
    private readonly IReviewService _reviews;
    private readonly ICreatorHelper _ch;
    private readonly ApplicationDbContext _db;
    private readonly INotificationService _notify;

    public IndexModel(IReviewService reviews, ICreatorHelper ch, ApplicationDbContext db, INotificationService notify)
    {
        _reviews = reviews;
        _ch = ch;
        _db = db;
        _notify = notify;
    }

    public List<AppReview> Reviews { get; set; } = new();
    public int PendingCount { get; set; }
    public int TotalCount { get; set; }

    [BindProperty(SupportsGet = true)] public string? FilterStatus { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    private const int PageSize = 20;

    private int _creatorId;

    public async Task<IActionResult> OnGetAsync()
    {
        var cid = await _ch.GetCreatorProfileIdAsync(User);
        if (cid == null) return Forbid();
        _creatorId = cid.Value;

        PendingCount = await _reviews.GetPendingCountForCreatorAsync(_creatorId);

        if (FilterStatus == "pending")
            Reviews = await _reviews.GetPendingReviewsForCreatorAsync(_creatorId, PageNumber, PageSize);
        else
            Reviews = await _reviews.GetAllReviewsForCreatorAsync(_creatorId, PageNumber, PageSize);

        TotalCount = Reviews.Count;
        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(int id)
    {
        var cid = await _ch.GetCreatorProfileIdAsync(User);
        if (cid == null) return Forbid();

        var review = await _db.Reviews.Include(r => r.App).FirstOrDefaultAsync(r => r.Id == id);
        var result = await _reviews.ApproveAsync(id, cid.Value);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess ? "نظر تایید و منتشر شد." : result.ErrorMessage;
        if (result.IsSuccess && review != null)
            await _notify.CreateAsync(review.UserId,
                $"نظر شما تایید شد: {review.App.Title}",
                "نظر شما توسط سازنده ابزار تایید و منتشر شد.",
                $"/app/{review.App.Slug}", "review");
        return RedirectToPage(new { FilterStatus, PageNumber });
    }

    public async Task<IActionResult> OnPostRejectAsync(int id)
    {
        var cid = await _ch.GetCreatorProfileIdAsync(User);
        if (cid == null) return Forbid();

        var review = await _db.Reviews.Include(r => r.App).FirstOrDefaultAsync(r => r.Id == id);
        var result = await _reviews.RejectAsync(id, cid.Value);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess ? "نظر رد شد." : result.ErrorMessage;
        if (result.IsSuccess && review != null)
            await _notify.CreateAsync(review.UserId,
                $"نظر شما رد شد: {review.App.Title}",
                "نظر ارسالی شما توسط سازنده ابزار تایید نشد.",
                $"/app/{review.App.Slug}", "review");
        return RedirectToPage(new { FilterStatus, PageNumber });
    }

    public async Task<IActionResult> OnPostReplyAsync(int id, string reply)
    {
        var cid = await _ch.GetCreatorProfileIdAsync(User);
        if (cid == null) return Forbid();

        var result = await _reviews.ReplyAsync(id, cid.Value, reply);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess ? "پاسخ ثبت شد." : result.ErrorMessage;
        return RedirectToPage(new { FilterStatus, PageNumber });
    }
}
