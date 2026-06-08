namespace PromptMarketPlace.Models.Domain;

public class AppCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? IconClass { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }

    public ICollection<AiApp> Apps { get; set; } = new List<AiApp>();
}
