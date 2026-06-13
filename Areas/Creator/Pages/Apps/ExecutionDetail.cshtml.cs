using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.Creator.Pages.Apps;

public class ExecutionDetailModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly ICreatorHelper _ch;

    public ExecutionDetailModel(ApplicationDbContext db, ICreatorHelper ch)
    {
        _db = db;
        _ch = ch;
    }

    public AppExecution Execution { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int appId, long id)
    {
        var creatorId = await _ch.GetCreatorProfileIdAsync(User);
        if (creatorId == null) return Forbid();

        var exec = await _db.Executions
            .Include(e => e.App).ThenInclude(a => a!.InputFields)
            .Include(e => e.InputValues)
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == id && e.AppId == appId && e.App!.CreatorProfileId == creatorId.Value);

        if (exec == null) return NotFound();
        Execution = exec;
        return Page();
    }
}
