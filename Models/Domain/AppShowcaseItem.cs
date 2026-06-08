using PromptMarketPlace.Models.Enums;

namespace PromptMarketPlace.Models.Domain;

public class AppShowcaseItem
{
    public int Id { get; set; }
    public int AppId { get; set; }
    public OutputType OutputType { get; set; }
    public string? OutputUrl { get; set; }
    public string? OutputText { get; set; }
    public string? Caption { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AiApp App { get; set; } = null!;
}
