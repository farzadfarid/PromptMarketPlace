namespace PromptMarketPlace.Models.Domain;

public class CreatorProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? WebsiteUrl { get; set; }
    public bool IsVerified { get; set; }
    public bool IsFoundingCreator { get; set; }
    public decimal CommissionPercent { get; set; } = 70;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
    public ICollection<AiApp> Apps { get; set; } = new List<AiApp>();
    public ICollection<WithdrawalRequest> WithdrawalRequests { get; set; } = new List<WithdrawalRequest>();
}
