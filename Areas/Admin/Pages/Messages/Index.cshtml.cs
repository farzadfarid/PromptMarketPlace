using Microsoft.AspNetCore.Mvc.RazorPages;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.Admin.Pages.Messages;

public class AdminMessagesIndexModel : PageModel
{
    private readonly IMessageService _msg;
    public AdminMessagesIndexModel(IMessageService msg) => _msg = msg;

    public List<MessageThread> Threads { get; set; } = new();

    public async Task OnGetAsync()
    {
        Threads = await _msg.GetAdminThreadsAsync();
    }
}
