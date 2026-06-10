using PromptMarketPlace.Models;
using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Services.Interfaces;

public interface IExecutionService
{
    Task<ExecutionResult> ExecuteAsync(string userId, int appId, Dictionary<string, string> inputs, List<string>? inputImageUrls = null);
    Task<AppExecution?> GetExecutionAsync(long id, string userId);
    Task<List<AppExecution>> GetUserExecutionsAsync(string userId, int page = 1, int pageSize = 20);
    Task<List<AppExecution>> GetAppExecutionsAsync(int appId, int creatorProfileId, int page = 1, int pageSize = 20);
    Task<ExecutionResult> RefundExecutionAsync(long id, string adminUserId);
}
