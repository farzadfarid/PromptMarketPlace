namespace PromptMarketPlace.Models.Domain;

public class MessageThread
{
    public int Id { get; set; }
    public int CreatorProfileId { get; set; }
    public int? AppId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

    public CreatorProfile Creator { get; set; } = null!;
    public AiApp? App { get; set; }
    public ICollection<ThreadMessage> Messages { get; set; } = new List<ThreadMessage>();
}
