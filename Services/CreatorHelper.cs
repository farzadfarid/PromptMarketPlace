using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Services;

public class CreatorHelper : ICreatorHelper
{
    private readonly ApplicationDbContext _db;
    public CreatorHelper(ApplicationDbContext db) => _db = db;

    public async Task<int?> GetCreatorProfileIdAsync(ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return null;
        return await _db.CreatorProfiles
            .Where(c => c.UserId == userId)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<CreatorProfile?> GetCreatorProfileAsync(ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return null;
        return await _db.CreatorProfiles
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }
}
