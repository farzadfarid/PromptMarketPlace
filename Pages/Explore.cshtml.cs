using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PromptMarketPlace.Models;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Pages;

public class ExploreModel : PageModel
{
    private readonly IAppService _apps;
    public ExploreModel(IAppService apps) => _apps = apps;

    public PagedResult<AiApp> Result { get; set; } = new();
    public List<AppCategory> Categories { get; set; } = new();

    [BindProperty(SupportsGet = true)] public int? CategoryId { get; set; }
    [BindProperty(SupportsGet = true)] public OutputType? OutputType { get; set; }
    [BindProperty(SupportsGet = true)] public int? MaxCost { get; set; }
    [BindProperty(SupportsGet = true)] public double? MinRating { get; set; }
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public AppSortBy SortBy { get; set; } = AppSortBy.MostUsed;
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;

    public async Task OnGetAsync()
    {
        Categories = await _apps.GetCategoriesAsync();
        Result = await _apps.GetPublishedAppsAsync(new AppFilterDto
        {
            CategoryId = CategoryId,
            OutputType = OutputType,
            MaxCreditCost = MaxCost,
            MinRating = MinRating,
            Search = Search,
            SortBy = SortBy,
            Page = PageNumber,
            PageSize = 20
        });
    }
}
