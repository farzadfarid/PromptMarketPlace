using PromptMarketPlace.Models.Enums;

namespace PromptMarketPlace.Models.Domain;

public class Payment
{
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int PackageId { get; set; }
    public decimal Amount { get; set; }
    public int CreditAmount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? ZarinpalAuthority { get; set; }
    public string? ZarinpalRefId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? VerifiedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public CreditPackage Package { get; set; } = null!;
}
