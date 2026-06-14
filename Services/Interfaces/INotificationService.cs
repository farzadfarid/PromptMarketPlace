using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Services.Interfaces;

public interface INotificationService
{
    Task CreateAsync(string userId, string title, string? message = null, string? link = null, string category = "general");
    Task CreateForAdminsAsync(string title, string? message = null, string? link = null, string category = "general");
    Task<int> GetUnreadCountAsync(string userId);
    Task<List<Notification>> GetRecentAsync(string userId, int count = 10);
    Task MarkAllReadAsync(string userId);
}
