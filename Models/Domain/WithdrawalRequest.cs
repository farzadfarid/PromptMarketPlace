using PromptMarketPlace.Models.Enums;

namespace PromptMarketPlace.Models.Domain;

public class WithdrawalRequest
{
    public int Id { get; set; }
    public int CreatorProfileId { get; set; }
    public decimal Amount { get; set; }
    public WithdrawalStatus Status { get; set; } = WithdrawalStatus.Pending;
    /// <summary>JSON: {"sheba":"IR...","accountOwner":"نام صاحب حساب"}</summary>
    public string BankAccountInfo { get; set; } = string.Empty;
    public string? AdminNote { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }

    public CreatorProfile Creator { get; set; } = null!;
}
