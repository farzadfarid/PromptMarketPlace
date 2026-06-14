namespace PromptMarketPlace.Models.Domain;

public class Notification
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string? Link { get; set; }
    public string Category { get; set; } = "general"; // app_review | app_status | open_prompt | review | withdrawal | general
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
}
