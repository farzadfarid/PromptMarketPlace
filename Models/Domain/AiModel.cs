namespace PromptMarketPlace.Models.Domain;

public class AiModel
{
    public int Id { get; set; }
    public int AiProviderId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public string? Description { get; set; }
    /// <summary>JSON array of AiCapability values e.g. ["TextGeneration","CodeGeneration"]</summary>
    public string Capabilities { get; set; } = "[]";
    public decimal? CostPer1KTokens { get; set; }
    public decimal? CostPerImage { get; set; }
    public decimal? CostPerSecondVideo { get; set; }
    public int? MaxTokens { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }

    public AiProvider Provider { get; set; } = null!;
    public ICollection<AiApp> Apps { get; set; } = new List<AiApp>();
}
