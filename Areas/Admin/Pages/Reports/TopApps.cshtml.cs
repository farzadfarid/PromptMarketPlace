using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;

namespace PromptMarketPlace.Areas.Admin.Pages.Reports;

public class TopAppsModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public TopAppsModel(ApplicationDbContext db) => _db = db;

    public List<AiApp> Apps { get; set; } = new();

    public async Task OnGetAsync()
    {
        Apps = await _db.Apps
            .Include(a => a.Creator).ThenInclude(c => c.User)
            .Include(a => a.Category)
            .Where(a => a.Status == AppStatus.Active)
            .OrderByDescending(a => a.ExecutionCount)
            .Take(50)
            .ToListAsync();
    }
}
