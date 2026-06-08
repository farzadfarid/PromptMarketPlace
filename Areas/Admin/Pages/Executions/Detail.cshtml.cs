using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Areas.Admin.Pages.Executions;

[Authorize(Roles = "Admin")]
public class DetailModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public DetailModel(ApplicationDbContext db) => _db = db;

    public AppExecution Execution { get; set; } = null!;
    public string ReturnUserId { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(long id, string? userId)
    {
        var exec = await _db.Executions
            .Include(e => e.App).ThenInclude(a => a!.InputFields)
            .Include(e => e.InputValues)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (exec == null) return NotFound();
        Execution = exec;
        ReturnUserId = userId ?? exec.UserId;
        return Page();
    }
}
