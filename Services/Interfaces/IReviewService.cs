using PromptMarketPlace.Models;
using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Services.Interfaces;

public interface IReviewService
{
    Task<List<AppReview>> GetAppReviewsAsync(int appId, int page = 1, int pageSize = 10);
    Task<List<AppReview>> GetPendingReviewsForCreatorAsync(int creatorProfileId, int page = 1, int pageSize = 20);
    Task<List<AppReview>> GetAllReviewsForCreatorAsync(int creatorProfileId, int page = 1, int pageSize = 20);
    Task<int> GetPendingCountForCreatorAsync(int creatorProfileId);
    Task<ExecutionResult> AddReviewAsync(string userId, int appId, int rating, string? comment);
    Task<bool> HasUserReviewedAsync(string userId, int appId);
    Task<ExecutionResult> ApproveAsync(int reviewId, int? creatorProfileId = null);
    Task<ExecutionResult> RejectAsync(int reviewId, int? creatorProfileId = null);
    Task<ExecutionResult> ReplyAsync(int reviewId, int creatorProfileId, string reply);
}
