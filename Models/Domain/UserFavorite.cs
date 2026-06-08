namespace PromptMarketPlace.Models.Domain;

public class UserFavorite
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int AppId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
    public AiApp App { get; set; } = null!;
}
