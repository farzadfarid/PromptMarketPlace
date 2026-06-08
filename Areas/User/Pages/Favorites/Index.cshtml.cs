using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Areas.User.Pages.Favorites;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public List<UserFavorite> Favorites { get; set; } = new();

    public async Task OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        Favorites = await _db.Favorites
            .Include(f => f.App).ThenInclude(a => a.Creator).ThenInclude(c => c.User)
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostToggleAsync(int appId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var existing = await _db.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.AppId == appId);

        if (existing != null)
            _db.Favorites.Remove(existing);
        else
            _db.Favorites.Add(new UserFavorite { UserId = userId, AppId = appId });

        await _db.SaveChangesAsync();
        return RedirectToPage();
    }
}

