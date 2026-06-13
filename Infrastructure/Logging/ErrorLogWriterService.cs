using PromptMarketPlace.Data;

namespace PromptMarketPlace.Infrastructure.Logging;

public sealed class ErrorLogWriterService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ErrorLogWriterService(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await foreach (var entry in ErrorLogChannel.Channel.Reader.ReadAllAsync(ct))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.ErrorLogs.Add(entry);
                await db.SaveChangesAsync(ct);
            }
            catch
            {
                // هرگز نباید اپ را crash کند
            }
        }
    }
}
