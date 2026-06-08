using Microsoft.AspNetCore.Identity;
using PromptMarketPlace.Models.Enums;

namespace PromptMarketPlace.Models.Domain;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public UserWallet? Wallet { get; set; }
    public CreatorProfile? CreatorProfile { get; set; }
    public ICollection<AppExecution> Executions { get; set; } = new List<AppExecution>();
    public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
}
