using PromptMarketPlace.Models.Enums;

namespace PromptMarketPlace.Models.Domain;

public class AppInputField
{
    public int Id { get; set; }
    public int AppId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Placeholder { get; set; }
    public string? HelpText { get; set; }
    public FieldType Type { get; set; } = FieldType.Text;
    /// <summary>JSON array for Select type: [{"value":"fa","label":"فارسی"}]</summary>
    public string? Options { get; set; }
    public bool IsRequired { get; set; } = true;
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public int SortOrder { get; set; }

    public AiApp App { get; set; } = null!;
}
