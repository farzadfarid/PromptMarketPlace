namespace PromptMarketPlace.Models.Domain;

public class ErrorLog
{
    public long Id { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ExceptionType { get; set; }
    public string? StackTrace { get; set; }
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsResolved { get; set; }
    public string? AiAnalysis { get; set; }
    public DateTime? AiAnalyzedAt { get; set; }
}
