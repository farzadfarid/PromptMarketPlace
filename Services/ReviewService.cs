using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Services;

public class ReviewService : IReviewService
{
    private readonly ApplicationDbContext _db;
    private readonly IAppService _appService;

    public ReviewService(ApplicationDbContext db, IAppService appService)
    {
        _db = db;
        _appService = appService;
    }

    public async Task<List<AppReview>> GetAppReviewsAsync(int appId, int page = 1, int pageSize = 10)
        => await _db.Reviews
            .Include(r => r.User)
            .Where(r => r.AppId == appId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<List<AppReview>> GetPendingReviewsForCreatorAsync(int creatorProfileId, int page = 1, int pageSize = 20)
        => await _db.Reviews
            .Include(r => r.User)
            .Include(r => r.App)
            .Where(r => r.App.CreatorProfileId == creatorProfileId && !r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<List<AppReview>> GetAllReviewsForCreatorAsync(int creatorProfileId, int page = 1, int pageSize = 20)
        => await _db.Reviews
            .Include(r => r.User)
            .Include(r => r.App)
            .Where(r => r.App.CreatorProfileId == creatorProfileId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<int> GetPendingCountForCreatorAsync(int creatorProfileId)
        => await _db.Reviews
            .CountAsync(r => r.App.CreatorProfileId == creatorProfileId && !r.IsApproved);

    public async Task<ExecutionResult> AddReviewAsync(string userId, int appId, int rating, string? comment)
    {
        if (rating < 1 || rating > 5)
            return ExecutionResult.Fail("امتیاز باید بین ۱ تا ۵ باشد.");

        var appExists = await _db.Apps.AnyAsync(a => a.Id == appId && a.Status == PromptMarketPlace.Models.Enums.AppStatus.Active);
        if (!appExists)
            return ExecutionResult.Fail("ابزار یافت نشد.");

        if (await HasUserReviewedAsync(userId, appId))
            return ExecutionResult.Fail("شما قبلاً برای این ابزار نظر ثبت کرده‌اید.");

        var hasUsed = await _db.Executions
            .AnyAsync(e => e.AppId == appId && e.UserId == userId &&
                           e.Status == PromptMarketPlace.Models.Enums.ExecutionStatus.Completed);

        _db.Reviews.Add(new AppReview
        {
            AppId = appId,
            UserId = userId,
            Rating = rating,
            Comment = comment?.Trim(),
            IsVerifiedPurchase = hasUsed,
            IsApproved = false
        });

        await _db.SaveChangesAsync();
        return ExecutionResult.Success(null!);
    }

    public async Task<bool> HasUserReviewedAsync(string userId, int appId)
        => await _db.Reviews.AnyAsync(r => r.UserId == userId && r.AppId == appId);

    public async Task<ExecutionResult> ApproveAsync(int reviewId, int? creatorProfileId = null)
    {
        var review = creatorProfileId.HasValue
            ? await _db.Reviews.Include(r => r.App)
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.App.CreatorProfileId == creatorProfileId.Value)
            : await _db.Reviews.FindAsync(reviewId);

        if (review == null) return ExecutionResult.Fail("نظر یافت نشد.");

        review.IsApproved = true;
        await _db.SaveChangesAsync();
        await _appService.RecalculateRatingAsync(review.AppId);
        return ExecutionResult.Success(null!);
    }

    public async Task<ExecutionResult> RejectAsync(int reviewId, int? creatorProfileId = null)
    {
        var review = creatorProfileId.HasValue
            ? await _db.Reviews.Include(r => r.App)
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.App.CreatorProfileId == creatorProfileId.Value)
            : await _db.Reviews.FindAsync(reviewId);

        if (review == null) return ExecutionResult.Fail("نظر یافت نشد.");

        var appId = review.AppId;
        _db.Reviews.Remove(review);
        await _db.SaveChangesAsync();
        await _appService.RecalculateRatingAsync(appId);
        return ExecutionResult.Success(null!);
    }

    public async Task<ExecutionResult> ReplyAsync(int reviewId, int creatorProfileId, string reply)
    {
        if (string.IsNullOrWhiteSpace(reply))
            return ExecutionResult.Fail("پاسخ نمی‌تواند خالی باشد.");

        var review = await _db.Reviews.Include(r => r.App)
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.App.CreatorProfileId == creatorProfileId && r.IsApproved);

        if (review == null) return ExecutionResult.Fail("نظر یافت نشد.");

        review.CreatorReply = reply.Trim();
        review.RepliedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ExecutionResult.Success(null!);
    }
}
