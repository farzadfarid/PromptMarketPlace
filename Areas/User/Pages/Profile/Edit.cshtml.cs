using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.User.Pages.Profile;

[Authorize]
public class EditModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IStorageService _storage;

    public EditModel(UserManager<ApplicationUser> userManager,
                     SignInManager<ApplicationUser> signInManager,
                     IStorageService storage)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _storage = storage;
    }

    [BindProperty] public string DisplayName { get; set; } = string.Empty;
    [BindProperty] public string? AvatarUrl { get; set; }
    [BindProperty] public string? CurrentPassword { get; set; }
    [BindProperty] public string? NewPassword { get; set; }
    [BindProperty] public string? ConfirmPassword { get; set; }

    public string Email { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();
        DisplayName = user.DisplayName;
        AvatarUrl = user.AvatarUrl;
        Email = user.Email ?? string.Empty;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(DisplayName))
            ModelState.AddModelError(nameof(DisplayName), "نام نمایشی الزامی است.");

        if (!ModelState.IsValid) return Page();

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        // handle avatar file upload
        var avatarFile = Request.Form.Files["AvatarFile"];
        if (avatarFile != null && avatarFile.Length > 0)
        {
            var allowed = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
            if (!allowed.Contains(avatarFile.ContentType))
            {
                ModelState.AddModelError(string.Empty, "فقط فایل‌های JPG، PNG، WebP و GIF قابل قبول هستند.");
                return Page();
            }
            if (avatarFile.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(string.Empty, "حجم فایل نباید بیشتر از ۵ مگابایت باشد.");
                return Page();
            }
            AvatarUrl = await _storage.SaveUploadAsync(avatarFile, "avatars");
        }

        user.DisplayName = DisplayName.Trim();
        user.AvatarUrl = string.IsNullOrWhiteSpace(AvatarUrl) ? null : AvatarUrl.Trim();
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var err in updateResult.Errors)
                ModelState.AddModelError(string.Empty, err.Description);
            return Page();
        }

        if (!string.IsNullOrEmpty(CurrentPassword) && !string.IsNullOrEmpty(NewPassword))
        {
            if (NewPassword != ConfirmPassword)
            {
                ModelState.AddModelError(nameof(ConfirmPassword), "رمز عبور جدید و تکرار آن مطابقت ندارند.");
                return Page();
            }
            var pwResult = await _userManager.ChangePasswordAsync(user, CurrentPassword, NewPassword);
            if (!pwResult.Succeeded)
            {
                foreach (var err in pwResult.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);
                return Page();
            }
            await _signInManager.RefreshSignInAsync(user);
        }

        TempData["Success"] = "پروفایل با موفقیت به‌روز شد.";
        return RedirectToPage();
    }
}
