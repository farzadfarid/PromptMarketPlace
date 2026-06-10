using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.Admin.Pages.Messages;

public class AdminThreadModel : PageModel
{
    private readonly IMessageService _msg;
    public AdminThreadModel(IMessageService msg) => _msg = msg;

    public MessageThread Thread { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var thread = await _msg.GetThreadAsync(id);
        if (thread == null) return NotFound();
        Thread = thread;
        await _msg.MarkReadAsync(id, byAdmin: true);
        return Page();
    }

    public async Task<IActionResult> OnPostSendAsync(int id, string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return RedirectToPage(new { id });
        var thread = await _msg.GetThreadAsync(id);
        if (thread == null) return NotFound();
        await _msg.SendAsync(id, isFromAdmin: true, content: content.Trim());
        return RedirectToPage(new { id });
    }
}
