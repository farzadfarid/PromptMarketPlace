using System.Security.Claims;
using PromptMarketPlace.Models.Domain;
using Serilog.Core;
using Serilog.Events;

namespace PromptMarketPlace.Infrastructure.Logging;

public sealed class DatabaseSink : ILogEventSink
{
    private readonly IHttpContextAccessor _httpCtx;

    public DatabaseSink(IServiceProvider services)
    {
        _httpCtx = services.GetRequiredService<IHttpContextAccessor>();
    }

    public void Emit(LogEvent logEvent)
    {
        if (logEvent.Level < LogEventLevel.Warning) return;

        try
        {
            var ctx = _httpCtx.HttpContext;
            var category = logEvent.Properties.TryGetValue("SourceContext", out var src)
                ? src.ToString().Trim('"') : "System";

            var entry = new ErrorLog
            {
                Level        = logEvent.Level.ToString(),
                Category     = category,
                Message      = logEvent.RenderMessage(),
                ExceptionType = logEvent.Exception?.GetType().FullName,
                StackTrace   = logEvent.Exception?.ToString(),
                RequestPath  = ctx?.Request.Path.Value,
                RequestMethod = ctx?.Request.Method,
                UserId       = ctx?.User?.FindFirstValue(ClaimTypes.NameIdentifier),
                UserName     = ctx?.User?.FindFirstValue("DisplayName") ?? ctx?.User?.Identity?.Name,
                CreatedAt    = logEvent.Timestamp.UtcDateTime
            };

            ErrorLogChannel.Channel.Writer.TryWrite(entry);
        }
        catch
        {
            // سینک لاگ هرگز نباید اپ را crash کند
        }
    }
}
