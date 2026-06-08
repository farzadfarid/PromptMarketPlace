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
            .Where(r => r.AppId == appId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<ExecutionResult> AddReviewAsync(string userId, int appId, int rating, string? comment)
    {
        if (rating < 1 || rating > 5)
            return ExecutionResult.Fail("امتیاز باید بین ۱ تا ۵ باشد.");

        var appExists = await _db.Apps.AnyAsync(a => a.Id == appId && a.Status == PromptMarketPlace.Models.Enums.AppStatus.Active);
        if (!appExists)
            return ExecutionResult.Fail("ابزار یافت نشد.");

        if (await HasUserReviewedAsync(userId, appId))
            return ExecutionResult.Fail("شما قبلاً برای این ابزار نظر ثبت کرده‌اید.");

        // فقط کاربری که حداقل یک بار اجرا کرده می‌تواند نظر بدهد
        var hasUsed = await _db.Executions
            .AnyAsync(e => e.AppId == appId && e.UserId == userId &&
                           e.Status == PromptMarketPlace.Models.Enums.ExecutionStatus.Completed);

        _db.Reviews.Add(new AppReview
        {
            AppId = appId,
            UserId = userId,
            Rating = rating,
            Comment = comment?.Trim(),
            IsVerifiedPurchase = hasUsed
        });

        await _db.SaveChangesAsync();
        await _appService.RecalculateRatingAsync(appId);

        return ExecutionResult.Success(null!);
    }

    public async Task<bool> HasUserReviewedAsync(string userId, int appId)
        => await _db.Reviews.AnyAsync(r => r.UserId == userId && r.AppId == appId);
}
