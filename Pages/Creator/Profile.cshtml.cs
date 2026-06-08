using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;

namespace PromptMarketPlace.Pages.Creator;

public class ProfileModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public ProfileModel(ApplicationDbContext db) => _db = db;

    public CreatorProfile Creator { get; set; } = null!;
    public List<AiApp> Apps { get; set; } = new();
    public long TotalExecutions { get; set; }
    public double AvgRating { get; set; }

    public async Task<IActionResult> OnGetAsync(string username)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == username);
        if (user == null) return NotFound();

        var creator = await _db.CreatorProfiles
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == user.Id);
        if (creator == null) return NotFound();

        Creator = creator;
        Apps = await _db.Apps
            .Where(a => a.CreatorProfileId == creator.Id && a.Status == AppStatus.Active)
            .OrderByDescending(a => a.ExecutionCount)
            .ToListAsync();

        TotalExecutions = Apps.Sum(a => a.ExecutionCount);
        AvgRating = Apps.Any() ? Apps.Average(a => a.AverageRating) : 0;
        return Page();
    }
}
