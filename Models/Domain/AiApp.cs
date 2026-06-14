using PromptMarketPlace.Models.Enums;

namespace PromptMarketPlace.Models.Domain;

public class AiApp
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public AppStatus Status { get; set; } = AppStatus.Draft;
    public int CreditCost { get; set; } = 1;
    public OutputType OutputType { get; set; }
    public int AiModelId { get; set; }
    public string EncryptedPrompt { get; set; } = string.Empty;
    public string? SystemContext { get; set; }
    public int CategoryId { get; set; }
    public int CreatorProfileId { get; set; }
    public long ExecutionCount { get; set; }
    public double AverageRating { get; set; }
    public bool IsPromptPublicRequested { get; set; }
    public bool IsPromptPublic { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public AiModel AiModel { get; set; } = null!;
    public AppCategory Category { get; set; } = null!;
    public CreatorProfile Creator { get; set; } = null!;
    public ICollection<AppInputField> InputFields { get; set; } = new List<AppInputField>();
    public ICollection<AppExecution> Executions { get; set; } = new List<AppExecution>();
    public ICollection<AppReview> Reviews { get; set; } = new List<AppReview>();
    public ICollection<AppShowcaseItem> ShowcaseItems { get; set; } = new List<AppShowcaseItem>();
    public ICollection<AppTag> Tags { get; set; } = new List<AppTag>();
}
