using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.Creator.Pages.Apps;

public class ExecutionsModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly ICreatorHelper _ch;

    public ExecutionsModel(ApplicationDbContext db, ICreatorHelper ch)
    {
        _db = db;
        _ch = ch;
    }

    public AiApp App { get; set; } = null!;
    public List<AppExecution> Executions { get; set; } = new();
    public int TotalCount { get; set; }

    [BindProperty(SupportsGet = true)] public ExecutionStatus? FilterStatus { get; set; }
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNum { get; set; } = 1;
    private const int PageSize = 20;

    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public async Task<IActionResult> OnGetAsync(int appId)
    {
        var creatorId = await _ch.GetCreatorProfileIdAsync(User);
        if (creatorId == null) return Forbid();

        var app = await _db.Apps
            .FirstOrDefaultAsync(a => a.Id == appId && a.CreatorProfileId == creatorId.Value);
        if (app == null) return NotFound();
        App = app;

        var q = _db.Executions
            .Include(e => e.User)
            .Where(e => e.AppId == appId)
            .AsQueryable();

        if (FilterStatus.HasValue)
            q = q.Where(e => e.Status == FilterStatus.Value);

        if (!string.IsNullOrWhiteSpace(Search))
            q = q.Where(e => e.User.DisplayName.Contains(Search));

        TotalCount = await q.CountAsync();
        Executions = await q
            .OrderByDescending(e => e.CreatedAt)
            .Skip((PageNum - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        return Page();
    }
}
