using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationService(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task CreateAsync(string userId, string title, string? message = null, string? link = null, string category = "general")
    {
        _db.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Link = link,
            Category = category,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    public async Task CreateForAdminsAsync(string title, string? message = null, string? link = null, string category = "general")
    {
        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        foreach (var admin in admins)
        {
            _db.Notifications.Add(new Notification
            {
                UserId = admin.Id,
                Title = title,
                Message = message,
                Link = link,
                Category = category,
                CreatedAt = DateTime.UtcNow
            });
        }
        if (admins.Any())
            await _db.SaveChangesAsync();
    }

    public Task<int> GetUnreadCountAsync(string userId) =>
        _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

    public Task<List<Notification>> GetRecentAsync(string userId, int count = 10) =>
        _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .ToListAsync();

    public async Task MarkAllReadAsync(string userId)
    {
        await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
    }
}
