namespace PromptMarketPlace.Models;

public class AiResponse
{
    public bool IsSuccess { get; set; }
    public string? Text { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string? AudioUrl { get; set; }
    public string? JobId { get; set; }
    public int TokensUsed { get; set; }
    public decimal ActualCost { get; set; }
    public string? ErrorMessage { get; set; }

    public static AiResponse Success(string text, int tokens = 0) =>
        new() { IsSuccess = true, Text = text, TokensUsed = tokens };

    public static AiResponse SuccessImage(string imageUrl) =>
        new() { IsSuccess = true, ImageUrl = imageUrl };

    public static AiResponse Fail(string error) =>
        new() { IsSuccess = false, ErrorMessage = error };
}
