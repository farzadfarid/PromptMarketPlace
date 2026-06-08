using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;

namespace PromptMarketPlace.Areas.Admin.Pages.Users;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public List<UserRow> Users { get; set; } = new();
    public int TotalCount { get; set; }

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public string? FilterRole { get; set; }
    [BindProperty(SupportsGet = true)] public string? FilterStatus { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    private const int PageSize = 30;

    public async Task OnGetAsync()
    {
        var query = _db.Users.AsQueryable();

        if (!string.IsNullOrEmpty(Search))
            query = query.Where(u => u.DisplayName.Contains(Search) || u.Email!.Contains(Search));

        if (!string.IsNullOrEmpty(FilterRole) && Enum.TryParse<UserRole>(FilterRole, out var role))
            query = query.Where(u => u.Role == role);

        if (FilterStatus == "active")
            query = query.Where(u => u.IsActive);
        else if (FilterStatus == "blocked")
            query = query.Where(u => !u.IsActive);

        TotalCount = await query.CountAsync();
        var raw = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .Select(u => new { u.Id, u.DisplayName, u.Email, u.Role, u.IsActive, u.CreatedAt })
            .ToListAsync();

        var ids = raw.Select(u => u.Id).ToList();
        var execCounts = await _db.Executions
            .Where(e => ids.Contains(e.UserId))
            .GroupBy(e => e.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count);

        Users = raw.Select(u => new UserRow(
            u.Id, u.DisplayName, u.Email ?? "", u.Role, u.IsActive, u.CreatedAt,
            execCounts.TryGetValue(u.Id, out var c) ? c : 0
        )).ToList();
    }

    public record UserRow(string Id, string DisplayName, string Email, UserRole Role,
        bool IsActive, DateTime CreatedAt, int ExecutionCount);
}
