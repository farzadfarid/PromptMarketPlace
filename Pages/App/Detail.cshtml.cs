using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Pages.App;

public class DetailModel : PageModel
{
    private readonly IAppService _apps;
    private readonly IExecutionService _execution;
    private readonly ICreditService _credits;
    private readonly IReviewService _reviews;

    public DetailModel(IAppService apps, IExecutionService execution,
        ICreditService credits, IReviewService reviews)
    {
        _apps = apps;
        _execution = execution;
        _credits = credits;
        _reviews = reviews;
    }

    public AiApp App { get; set; } = null!;
    public AppExecution? LastExecution { get; set; }
    public List<AppReview> Reviews { get; set; } = new();
    public int UserBalance { get; set; }
    public bool HasReviewed { get; set; }
    public string? RunError { get; set; }

    [BindProperty]
    public Dictionary<string, string> Inputs { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string slug, long? exec = null)
    {
        var app = await _apps.GetAppBySlugAsync(slug);
        if (app == null) return NotFound();
        App = app;

        Reviews = await _reviews.GetAppReviewsAsync(app.Id, pageSize: 5);

        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            UserBalance = await _credits.GetBalanceAsync(userId);
            HasReviewed = await _reviews.HasUserReviewedAsync(userId, app.Id);

            if (exec.HasValue)
            {
                LastExecution = await _execution.GetExecutionAsync(exec.Value, userId);
                if (LastExecution?.Status == ExecutionStatus.Failed)
                    foreach (var iv in LastExecution.InputValues)
                        Inputs[iv.FieldName] = iv.FieldValue;
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostRunAsync(string slug)
    {
        if (!User.Identity?.IsAuthenticated == true)
            return RedirectToPage("/Auth/Login", new { returnUrl = $"/app/{slug}" });

        var app = await _apps.GetAppBySlugAsync(slug);
        if (app == null) return NotFound();
        App = app;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _execution.ExecuteAsync(userId, app.Id, Inputs);

        if (!result.IsSuccess)
        {
            RunError = result.ErrorMessage;
            UserBalance = await _credits.GetBalanceAsync(userId);
            Reviews = await _reviews.GetAppReviewsAsync(app.Id, pageSize: 5);
            return Page();
        }

        return RedirectToPage("/App/Detail", new { slug, exec = result.Execution!.Id });
    }

    public async Task<IActionResult> OnPostReviewAsync(string slug, int rating, string? comment)
    {
        if (!User.Identity?.IsAuthenticated == true) return Forbid();

        var app = await _apps.GetAppBySlugAsync(slug);
        if (app == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _reviews.AddReviewAsync(userId, app.Id, rating, comment);

        return RedirectToPage("/App/Detail", new { slug });
    }
}
