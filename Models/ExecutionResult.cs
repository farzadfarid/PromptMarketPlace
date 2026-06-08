using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Models;

public class ExecutionResult
{
    public bool IsSuccess { get; private set; }
    public string? ErrorMessage { get; private set; }
    public AppExecution? Execution { get; private set; }

    public static ExecutionResult Success(AppExecution execution)
        => new() { IsSuccess = true, Execution = execution };

    public static ExecutionResult Fail(string error)
        => new() { IsSuccess = false, ErrorMessage = error };
}
