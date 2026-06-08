using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Areas.User.Pages.Profile;

[Authorize]
public class EditModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public EditModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [BindProperty] public string DisplayName { get; set; } = string.Empty;
    [BindProperty] public string? AvatarUrl { get; set; }
    [BindProperty] public string? CurrentPassword { get; set; }
    [BindProperty] public string? NewPassword { get; set; }
    [BindProperty] public string? ConfirmPassword { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();
        DisplayName = user.DisplayName;
        AvatarUrl = user.AvatarUrl;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(DisplayName))
            ModelState.AddModelError(nameof(DisplayName), "Ù†Ø§Ù… Ù†Ù…Ø§ÛŒØ´ÛŒ Ø§Ù„Ø²Ø§Ù…ÛŒ Ø§Ø³Øª.");

        if (!ModelState.IsValid) return Page();

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

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
                ModelState.AddModelError(nameof(ConfirmPassword), "Ø±Ù…Ø² Ø¹Ø¨ÙˆØ± Ø¬Ø¯ÛŒØ¯ Ùˆ ØªÚ©Ø±Ø§Ø± Ø¢Ù† Ù…Ø·Ø§Ø¨Ù‚Øª Ù†Ø¯Ø§Ø±Ù†Ø¯.");
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

        TempData["Success"] = "Ù¾Ø±ÙˆÙØ§ÛŒÙ„ Ø¨Ø§ Ù…ÙˆÙÙ‚ÛŒØª Ø¨Ù‡â€ŒØ±ÙˆØ² Ø´Ø¯.";
        return RedirectToPage();
    }
}

