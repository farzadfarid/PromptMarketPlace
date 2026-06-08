using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Areas.User.Pages.Executions;

[Authorize]
public class DetailModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public DetailModel(ApplicationDbContext db) => _db = db;

    public AppExecution Execution { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(long id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var exec = await _db.Executions
            .Include(e => e.App).ThenInclude(a => a!.InputFields)
            .Include(e => e.InputValues)
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

        if (exec == null) return NotFound();
        Execution = exec;
        return Page();
    }

    public async Task<IActionResult> OnPostTogglePublicAsync(long id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var exec = await _db.Executions.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        if (exec == null) return NotFound();

        exec.IsPublic = !exec.IsPublic;
        await _db.SaveChangesAsync();

        TempData["Success"] = exec.IsPublic ? "Ø®Ø±ÙˆØ¬ÛŒ Ø¹Ù…ÙˆÙ…ÛŒ Ø´Ø¯." : "Ø®Ø±ÙˆØ¬ÛŒ Ø®ØµÙˆØµÛŒ Ø´Ø¯.";
        return RedirectToPage(new { id });
    }
}

