namespace PromptMarketPlace.Models.Domain;

public class ExecutionInputValue
{
    public int Id { get; set; }
    public long ExecutionId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string FieldValue { get; set; } = string.Empty;

    public AppExecution Execution { get; set; } = null!;
}
