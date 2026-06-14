using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.Admin.Pages.Messages;

public class AdminThreadModel : PageModel
{
    private readonly IMessageService _msg;
    private readonly INotificationService _notify;

    public AdminThreadModel(IMessageService msg, INotificationService notify)
    {
        _msg = msg;
        _notify = notify;
    }

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

        // GetThreadAsync already includes Creator (with UserId)
        var creatorUserId = thread.Creator?.UserId;
        if (creatorUserId != null)
            await _notify.CreateAsync(creatorUserId,
                $"پاسخ جدید از ادمین: {thread.Subject}",
                content.Trim().Length > 80 ? content.Trim()[..80] + "…" : content.Trim(),
                $"/Creator/Messages/Thread?id={id}", "general");

        return RedirectToPage(new { id });
    }
}
