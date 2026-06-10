using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Services;

public class MessageService : IMessageService
{
    private readonly ApplicationDbContext _db;
    public MessageService(ApplicationDbContext db) => _db = db;

    public async Task<MessageThread> StartThreadAsync(int creatorProfileId, string subject, int? appId = null)
    {
        var thread = new MessageThread
        {
            CreatorProfileId = creatorProfileId,
            AppId = appId,
            Subject = subject,
            CreatedAt = DateTime.UtcNow,
            LastMessageAt = DateTime.UtcNow
        };
        _db.MessageThreads.Add(thread);
        await _db.SaveChangesAsync();
        return thread;
    }

    public async Task<MessageThread?> GetThreadAsync(int threadId)
        => await _db.MessageThreads
            .Include(t => t.Creator).ThenInclude(c => c.User)
            .Include(t => t.App)
            .Include(t => t.Messages.OrderBy(m => m.SentAt))
            .FirstOrDefaultAsync(t => t.Id == threadId);

    public async Task<List<MessageThread>> GetAdminThreadsAsync()
        => await _db.MessageThreads
            .Include(t => t.Creator).ThenInclude(c => c.User)
            .Include(t => t.App)
            .Include(t => t.Messages.OrderByDescending(m => m.SentAt).Take(1))
            .OrderByDescending(t => t.LastMessageAt)
            .ToListAsync();

    public async Task<List<MessageThread>> GetCreatorThreadsAsync(int creatorProfileId)
        => await _db.MessageThreads
            .Include(t => t.App)
            .Include(t => t.Messages.OrderByDescending(m => m.SentAt).Take(1))
            .Where(t => t.CreatorProfileId == creatorProfileId)
            .OrderByDescending(t => t.LastMessageAt)
            .ToListAsync();

    public async Task SendAsync(int threadId, bool isFromAdmin, string content)
    {
        _db.ThreadMessages.Add(new ThreadMessage
        {
            ThreadId = threadId,
            IsFromAdmin = isFromAdmin,
            Content = content,
            SentAt = DateTime.UtcNow,
            IsRead = false
        });
        var thread = await _db.MessageThreads.FindAsync(threadId);
        if (thread != null) thread.LastMessageAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task MarkReadAsync(int threadId, bool byAdmin)
    {
        // mark messages sent by the OTHER side as read
        var messages = await _db.ThreadMessages
            .Where(m => m.ThreadId == threadId && m.IsFromAdmin != byAdmin && !m.IsRead)
            .ToListAsync();
        foreach (var m in messages) m.IsRead = true;
        if (messages.Count > 0) await _db.SaveChangesAsync();
    }

    public async Task<int> UnreadCountAsync(bool forAdmin, int? creatorProfileId = null)
    {
        var query = _db.ThreadMessages
            .Where(m => m.IsFromAdmin != forAdmin && !m.IsRead);
        if (!forAdmin && creatorProfileId.HasValue)
            query = query.Where(m => m.Thread.CreatorProfileId == creatorProfileId.Value);
        return await query.CountAsync();
    }
}
