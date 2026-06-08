namespace PromptMarketPlace.Models.Domain;

public class AiProvider
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string? ApiKeyEncrypted { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<AiModel> Models { get; set; } = new List<AiModel>();
}
