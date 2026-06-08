using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;

namespace PromptMarketPlace.Services.Interfaces;

public interface ICreditService
{
    Task<int> GetBalanceAsync(string userId);
    Task<bool> HasEnoughCreditsAsync(string userId, int required);
    Task DeductCreditsAsync(string userId, int amount, string description, string? referenceId = null);
    Task AddCreditsAsync(string userId, int amount, string description, string? referenceId = null, TransactionType type = TransactionType.Purchase);
    Task DistributeEarningsAsync(AppExecution execution, AiApp app);
    Task EnsureWalletExistsAsync(string userId);
    Task<List<WalletTransaction>> GetTransactionHistoryAsync(string userId, int page = 1, int pageSize = 20);
}
