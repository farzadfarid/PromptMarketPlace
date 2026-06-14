using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.Creator.Pages.Messages;

public class CreatorThreadModel : PageModel
{
    private readonly IMessageService _msg;
    private readonly ICreatorHelper _ch;
    private readonly INotificationService _notify;

    public CreatorThreadModel(IMessageService msg, ICreatorHelper ch, INotificationService notify)
    {
        _msg = msg;
        _ch = ch;
        _notify = notify;
    }

    public MessageThread Thread { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var cid = await _ch.GetCreatorProfileIdAsync(User);
        if (cid == null) return Forbid();
        var thread = await _msg.GetThreadAsync(id);
        if (thread == null) return NotFound();
        if (thread.CreatorProfileId != cid.Value) return Forbid();
        Thread = thread;
        await _msg.MarkReadAsync(id, byAdmin: false);
        return Page();
    }

    public async Task<IActionResult> OnPostSendAsync(int id, string content)
    {
        var cid = await _ch.GetCreatorProfileIdAsync(User);
        if (cid == null) return Forbid();
        if (string.IsNullOrWhiteSpace(content)) return RedirectToPage(new { id });
        var thread = await _msg.GetThreadAsync(id);
        if (thread == null) return NotFound();
        if (thread.CreatorProfileId != cid.Value) return Forbid();
        await _msg.SendAsync(id, isFromAdmin: false, content: content.Trim());

        await _notify.CreateForAdminsAsync(
            $"پاسخ جدید از سازنده: {thread.Subject}",
            content.Trim().Length > 80 ? content.Trim()[..80] + "…" : content.Trim(),
            $"/Admin/Messages/Thread?id={id}", "general");

        return RedirectToPage(new { id });
    }
}
