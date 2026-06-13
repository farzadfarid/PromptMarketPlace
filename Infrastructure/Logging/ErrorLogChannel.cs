using System.Threading.Channels;
using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Infrastructure.Logging;

public static class ErrorLogChannel
{
    public static readonly Channel<ErrorLog> Channel =
        System.Threading.Channels.Channel.CreateBounded<ErrorLog>(
            new BoundedChannelOptions(2000)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = false
            });
}
