namespace PromptMarketPlace.Models.Domain;

public class UserWallet
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int CreditBalance { get; set; }
    public decimal EarningBalance { get; set; }
    public decimal TotalEarned { get; set; }
    public decimal TotalWithdrawn { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
