using PromptMarketPlace.Models.Enums;

namespace PromptMarketPlace.Models;

public class CreateAppDto
{
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public OutputType OutputType { get; set; }
    public int AiModelId { get; set; }
    public int CreditCost { get; set; } = 1;
    public string PlainTextPrompt { get; set; } = string.Empty;
    public string? SystemContext { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? ThumbnailUrl { get; set; }
}

public class UpdateAppDto
{
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int CreditCost { get; set; }
    public string? NewPlainTextPrompt { get; set; }
    public string? SystemContext { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? ThumbnailUrl { get; set; }
}
