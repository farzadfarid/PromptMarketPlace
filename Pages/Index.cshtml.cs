using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;

namespace PromptMarketPlace.Pages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public List<AiApp> FeaturedApps { get; set; } = new();
    public List<AiApp> NewApps { get; set; } = new();
    public List<(AppCategory Category, int Count)> Categories { get; set; } = new();
    public List<CreatorProfile> TopCreators { get; set; } = new();
    public int TotalApps { get; set; }
    public long TotalExecutions { get; set; }
    public int TotalCreators { get; set; }

    public async Task OnGetAsync()
    {
        var activeApps = _db.Apps.Where(a => a.Status == AppStatus.Active);

        FeaturedApps = await activeApps
            .Include(a => a.Creator).ThenInclude(c => c.User)
            .OrderByDescending(a => a.ExecutionCount)
            .Take(8).ToListAsync();

        NewApps = await activeApps
            .Include(a => a.Creator).ThenInclude(c => c.User)
            .OrderByDescending(a => a.CreatedAt)
            .Take(6).ToListAsync();

        var cats = await _db.Categories.OrderBy(c => c.SortOrder).Take(8).ToListAsync();
        var catCounts = await _db.Apps
            .Where(a => a.Status == AppStatus.Active)
            .GroupBy(a => a.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count);
        foreach (var cat in cats)
            Categories.Add((cat, catCounts.GetValueOrDefault(cat.Id, 0)));

        TopCreators = await _db.CreatorProfiles
            .Include(c => c.User)
            .Include(c => c.Apps)
            .Where(c => c.Apps.Any(a => a.Status == AppStatus.Active))
            .OrderByDescending(c => c.Apps.Sum(a => (long)a.ExecutionCount))
            .Take(6).ToListAsync();

        TotalApps = await activeApps.CountAsync();
        TotalExecutions = await _db.Executions
            .Where(e => e.Status == ExecutionStatus.Completed)
            .LongCountAsync();
        TotalCreators = await _db.CreatorProfiles.CountAsync();
    }
}
