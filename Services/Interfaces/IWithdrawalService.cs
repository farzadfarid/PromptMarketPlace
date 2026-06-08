using PromptMarketPlace.Models;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;

namespace PromptMarketPlace.Services.Interfaces;

public interface IWithdrawalService
{
    Task<ExecutionResult> RequestWithdrawalAsync(int creatorProfileId, decimal amount, string sheba, string accountOwner);
    Task<List<WithdrawalRequest>> GetCreatorRequestsAsync(int creatorProfileId);
    Task<List<WithdrawalRequest>> GetPendingRequestsAsync();
    Task<ExecutionResult> ProcessRequestAsync(int requestId, WithdrawalStatus newStatus, string? adminNote = null);
}
