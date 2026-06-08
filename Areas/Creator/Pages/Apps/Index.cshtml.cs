using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PromptMarketPlace.Models;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.Creator.Pages.Apps;

public class IndexModel : PageModel
{
    private readonly IAppService _apps;
    private readonly ICreatorHelper _creatorHelper;

    public IndexModel(IAppService apps, ICreatorHelper creatorHelper)
    {
        _apps = apps;
        _creatorHelper = creatorHelper;
    }

    public PagedResult<AiApp> Result { get; set; } = new();
    public int CreatorProfileId { get; set; }

    [BindProperty(SupportsGet = true)] public AppStatus? FilterStatus { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;

    public async Task<IActionResult> OnGetAsync()
    {
        var creatorId = await _creatorHelper.GetCreatorProfileIdAsync(User);
        if (creatorId == null) return RedirectToPage("/Index", new { area = "" });
        CreatorProfileId = creatorId.Value;

        Result = await _apps.GetAppsByCreatorAsync(CreatorProfileId, PageNumber, 20, FilterStatus);
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int appId)
    {
        var creatorId = await _creatorHelper.GetCreatorProfileIdAsync(User);
        if (creatorId == null) return Forbid();

        var result = await _apps.DeleteAppAsync(appId, creatorId.Value);
        TempData[result.IsSuccess ? "Success" : "Error"] =
            result.IsSuccess ? "ابزار حذف شد." : result.ErrorMessage;
        return RedirectToPage();
    }
}
