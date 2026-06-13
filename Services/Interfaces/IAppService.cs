using PromptMarketPlace.Models;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;

namespace PromptMarketPlace.Services.Interfaces;

public interface IAppService
{
    Task<PagedResult<AiApp>> GetPublishedAppsAsync(AppFilterDto filter);
    Task<AiApp?> GetAppBySlugAsync(string slug);
    Task<AiApp?> GetAppByIdAsync(int id);
    Task<PagedResult<AiApp>> GetAppsByCreatorAsync(int creatorProfileId, int page = 1, int pageSize = 20, AppStatus? status = null);

    Task<AiApp> CreateAppAsync(int creatorProfileId, CreateAppDto dto);
    Task<ExecutionResult> UpdateAppAsync(int appId, int creatorProfileId, UpdateAppDto dto);
    Task<ExecutionResult> SubmitForReviewAsync(int appId, int creatorProfileId);
    Task<ExecutionResult> UpdateStatusAsync(int appId, AppStatus newStatus, string? adminNote = null);
    Task<ExecutionResult> DeleteAppAsync(int appId, int creatorProfileId);
    Task RecalculateRatingAsync(int appId);

    Task<List<AppCategory>> GetCategoriesAsync();
    Task<List<AiApp>> GetSimilarAppsAsync(int appId, int categoryId, int count = 4);
}
