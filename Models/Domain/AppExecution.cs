using PromptMarketPlace.Models.Enums;

namespace PromptMarketPlace.Models.Domain;

public class AppExecution
{
    public long Id { get; set; }
    public int AppId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ExecutionStatus Status { get; set; } = ExecutionStatus.Pending;
    public OutputType OutputType { get; set; }
    public string? OutputText { get; set; }
    public string? OutputImageUrl { get; set; }
    public string? OutputVideoUrl { get; set; }
    public string? OutputAudioUrl { get; set; }
    /// <summary>JSON schema for dynamic form output type</summary>
    public string? OutputFormSchema { get; set; }
    public int CreditUsed { get; set; }
    public int? TokensUsed { get; set; }
    public decimal ActualApiCost { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan? Duration { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AiApp App { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    public ICollection<ExecutionInputValue> InputValues { get; set; } = new List<ExecutionInputValue>();
}
