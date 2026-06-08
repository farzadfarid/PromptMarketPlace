using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;

namespace PromptMarketPlace.Pages.Creators;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public List<CreatorProfile> Creators { get; set; } = new();

    public async Task OnGetAsync()
    {
        Creators = await _db.CreatorProfiles
            .Include(c => c.User)
            .Include(c => c.Apps)
            .Where(c => c.Apps.Any(a => a.Status == AppStatus.Active))
            .OrderByDescending(c => c.Apps.Sum(a => (long)a.ExecutionCount))
            .Take(50)
            .ToListAsync();
    }
}
