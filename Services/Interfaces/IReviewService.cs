using PromptMarketPlace.Models;
using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Services.Interfaces;

public interface IReviewService
{
    Task<List<AppReview>> GetAppReviewsAsync(int appId, int page = 1, int pageSize = 10);
    Task<ExecutionResult> AddReviewAsync(string userId, int appId, int rating, string? comment);
    Task<bool> HasUserReviewedAsync(string userId, int appId);
}
