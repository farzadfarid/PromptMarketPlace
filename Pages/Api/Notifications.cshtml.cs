using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Pages.Api;

[Authorize]
public class NotificationsModel : PageModel
{
    private readonly INotificationService _notify;

    public NotificationsModel(INotificationService notify) => _notify = notify;

    public IActionResult OnGet() => NotFound();

    public async Task<IActionResult> OnGetUnreadAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var count = await _notify.GetUnreadCountAsync(userId);
        return new JsonResult(new { count });
    }

    public async Task<IActionResult> OnGetRecentAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var items = await _notify.GetRecentAsync(userId, 10);
        var result = items.Select(n => new
        {
            n.Id, n.Title, n.Message, n.Link, n.Category, n.IsRead,
            CreatedAt = n.CreatedAt.ToString("o")
        });
        return new JsonResult(result);
    }

    public async Task<IActionResult> OnPostMarkAllReadAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _notify.MarkAllReadAsync(userId);
        return new JsonResult(new { ok = true });
    }
}
