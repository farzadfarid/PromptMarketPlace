using PromptMarketPlace.Models.Enums;

namespace PromptMarketPlace.Models.Domain;

public class WalletTransaction
{
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public int? CreditAmount { get; set; }
    public decimal? MoneyAmount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReferenceId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
}
