namespace PromptMarketPlace.Models.Domain;

public class ThreadMessage
{
    public int Id { get; set; }
    public int ThreadId { get; set; }
    public bool IsFromAdmin { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; }

    public MessageThread Thread { get; set; } = null!;
}
