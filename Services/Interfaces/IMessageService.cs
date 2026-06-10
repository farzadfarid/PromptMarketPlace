using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Services.Interfaces;

public interface IMessageService
{
    Task<MessageThread> StartThreadAsync(int creatorProfileId, string subject, int? appId = null);
    Task<MessageThread?> GetThreadAsync(int threadId);
    Task<List<MessageThread>> GetAdminThreadsAsync();
    Task<List<MessageThread>> GetCreatorThreadsAsync(int creatorProfileId);
    Task SendAsync(int threadId, bool isFromAdmin, string content);
    Task MarkReadAsync(int threadId, bool byAdmin);
    Task<int> UnreadCountAsync(bool forAdmin, int? creatorProfileId = null);
}
