using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.Creator.Pages.Profile;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly ICreatorHelper _ch;
    public EditModel(ApplicationDbContext db, ICreatorHelper ch) { _db = db; _ch = ch; }

    public CreatorProfile Creator { get; set; } = null!;
    [BindProperty] public ProfileForm Form { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var creator = await _ch.GetCreatorProfileAsync(User);
        if (creator == null) return NotFound();
        Creator = creator;
        Form = new ProfileForm { Bio = creator.Bio, WebsiteUrl = creator.WebsiteUrl };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var creator = await _ch.GetCreatorProfileAsync(User);
        if (creator == null) return NotFound();
        if (!ModelState.IsValid) { Creator = creator; return Page(); }

        creator.Bio = Form.Bio;
        creator.WebsiteUrl = Form.WebsiteUrl;
        await _db.SaveChangesAsync();

        TempData["Success"] = "پروفایل بروزرسانی شد.";
        return RedirectToPage();
    }

    public class ProfileForm
    {
        [MaxLength(500)] public string? Bio { get; set; }
        [Url][MaxLength(200)] public string? WebsiteUrl { get; set; }
    }
}
