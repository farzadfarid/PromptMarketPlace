using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.Creator.Pages.Messages;

public class CreatorMessagesIndexModel : PageModel
{
    private readonly IMessageService _msg;
    private readonly ICreatorHelper _ch;
    private readonly INotificationService _notify;

    public CreatorMessagesIndexModel(IMessageService msg, ICreatorHelper ch, INotificationService notify)
    {
        _msg = msg;
        _ch = ch;
        _notify = notify;
    }

    public List<MessageThread> Threads { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var cid = await _ch.GetCreatorProfileIdAsync(User);
        if (cid == null) return Forbid();
        Threads = await _msg.GetCreatorThreadsAsync(cid.Value);
        return Page();
    }

    public async Task<IActionResult> OnPostNewThreadAsync(string subject, string firstMessage)
    {
        var cid = await _ch.GetCreatorProfileIdAsync(User);
        if (cid == null) return Forbid();
        if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(firstMessage))
            return RedirectToPage();
        var thread = await _msg.StartThreadAsync(cid.Value, subject.Trim());
        await _msg.SendAsync(thread.Id, isFromAdmin: false, content: firstMessage.Trim());

        await _notify.CreateForAdminsAsync(
            $"پیام جدید از سازنده: {subject.Trim()}",
            firstMessage.Trim().Length > 80 ? firstMessage.Trim()[..80] + "…" : firstMessage.Trim(),
            $"/Admin/Messages/Thread?id={thread.Id}", "general");

        return RedirectToPage("/Messages/Thread", new { area = "Creator", id = thread.Id });
    }
}
