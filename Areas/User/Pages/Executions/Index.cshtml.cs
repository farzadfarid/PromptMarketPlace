using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;

namespace PromptMarketPlace.Areas.User.Pages.Executions;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public List<AppExecution> Executions { get; set; } = new();
    public int TotalCount { get; set; }

    [BindProperty(SupportsGet = true)] public ExecutionStatus? FilterStatus { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    private const int PageSize = 20;

    public async Task OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var query = _db.Executions
            .Include(e => e.App)
            .Where(e => e.UserId == userId)
            .AsQueryable();

        if (FilterStatus.HasValue)
            query = query.Where(e => e.Status == FilterStatus.Value);

        TotalCount = await query.CountAsync();
        Executions = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();
    }
}

