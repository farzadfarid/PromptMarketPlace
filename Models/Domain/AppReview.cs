namespace PromptMarketPlace.Models.Domain;

public class AppReview
{
    public int Id { get; set; }
    public int AppId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public bool IsApproved { get; set; } = false;
    public string? CreatorReply { get; set; }
    public DateTime? RepliedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AiApp App { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
