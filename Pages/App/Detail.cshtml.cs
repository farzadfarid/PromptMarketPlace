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
    private readonly IStorageService _storage;
    private readonly ILogger<DetailModel> _logger;

    public DetailModel(IAppService apps, IExecutionService execution,
        ICreditService credits, IReviewService reviews,
        IStorageService storage, ILogger<DetailModel> logger)
    {
        _apps = apps;
        _execution = execution;
        _credits = credits;
        _reviews = reviews;
        _storage = storage;
        _logger = logger;
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

        // Process uploaded files for FileUpload fields
        var imageUrls = new List<string>();
        foreach (var formFile in Request.Form.Files)
        {
            if (!formFile.Name.StartsWith("Files[") || !formFile.Name.EndsWith("]")) continue;
            if (formFile.Length == 0) continue;

            var fieldName = formFile.Name[6..^1];
            try
            {
                var relativePath = await _storage.SaveUploadAsync(formFile, "inputs");
                Inputs[fieldName] = relativePath;
                imageUrls.Add($"{Request.Scheme}://{Request.Host}{relativePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save uploaded file for field {Field}", fieldName);
            }
        }

        var result = await _execution.ExecuteAsync(userId, app.Id, Inputs,
            imageUrls.Count > 0 ? imageUrls : null);

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
        var result = await _reviews.AddReviewAsync(userId, app.Id, rating, comment);

        if (result.IsSuccess)
            TempData["ReviewPending"] = "true";
        else
            TempData["ReviewError"] = result.ErrorMessage;

        return RedirectToPage("/App/Detail", new { slug });
    }
}
