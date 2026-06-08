namespace PromptMarketPlace.Models.Domain;

public class AdminAuditLog
{
    public long Id { get; set; }
    public string AdminUserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? TargetType { get; set; }
    public string? TargetId { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser Admin { get; set; } = null!;
}
