using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.Creator.Pages.Profile;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly ICreatorHelper _ch;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStorageService _storage;

    public EditModel(ApplicationDbContext db, ICreatorHelper ch,
                     UserManager<ApplicationUser> userManager, IStorageService storage)
    {
        _db = db;
        _ch = ch;
        _userManager = userManager;
        _storage = storage;
    }

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

        // handle avatar file upload
        var avatarFile = Request.Form.Files["AvatarFile"];
        if (avatarFile != null && avatarFile.Length > 0)
        {
            var allowed = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
            if (!allowed.Contains(avatarFile.ContentType))
            {
                ModelState.AddModelError(string.Empty, "فقط فایل‌های JPG، PNG، WebP و GIF قابل قبول هستند.");
                Creator = creator;
                return Page();
            }
            if (avatarFile.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(string.Empty, "حجم فایل نباید بیشتر از ۵ مگابایت باشد.");
                Creator = creator;
                return Page();
            }
            var relativePath = await _storage.SaveUploadAsync(avatarFile, "avatars");
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.AvatarUrl = relativePath;
                await _userManager.UpdateAsync(user);
            }
        }

        TempData["Success"] = "پروفایل بروزرسانی شد.";
        return RedirectToPage();
    }

    public class ProfileForm
    {
        [MaxLength(500)] public string? Bio { get; set; }
        [Url][MaxLength(200)] public string? WebsiteUrl { get; set; }
    }
}
