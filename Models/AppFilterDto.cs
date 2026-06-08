using PromptMarketPlace.Models.Enums;

namespace PromptMarketPlace.Models;

public class AppFilterDto
{
    public int? CategoryId { get; set; }
    public OutputType? OutputType { get; set; }
    public int? MinCreditCost { get; set; }
    public int? MaxCreditCost { get; set; }
    public double? MinRating { get; set; }
    public string? Search { get; set; }
    public string? TagName { get; set; }
    public AppSortBy SortBy { get; set; } = AppSortBy.MostUsed;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public enum AppSortBy
{
    Newest,
    MostUsed,
    HighestRated,
    LowestCost
}
