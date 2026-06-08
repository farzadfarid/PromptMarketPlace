using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Pages.Auth;

public class ResetPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ResetPasswordModel(UserManager<ApplicationUser> userManager)
        => _userManager = userManager;

    [BindProperty] public string Email { get; set; } = string.Empty;
    [BindProperty] public string Token { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "رمز عبور جدید الزامی است")]
    [MinLength(8, ErrorMessage = "رمز عبور حداقل ۸ کاراکتر باشد")]
    public string NewPassword { get; set; } = string.Empty;

    [BindProperty]
    [Compare("NewPassword", ErrorMessage = "رمزهای عبور مطابقت ندارند")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public bool InvalidLink { get; set; }
    public bool Success { get; set; }

    public IActionResult OnGet(string? email, string? token)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
        {
            InvalidLink = true;
            return Page();
        }
        Email = email;
        Token = token;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var user = await _userManager.FindByEmailAsync(Email);
        if (user == null)
        {
            // برای امنیت پیام موفقیت نشان می‌دهیم
            Success = true;
            return Page();
        }

        var result = await _userManager.ResetPasswordAsync(user, Token, NewPassword);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError("", e.Description);
            return Page();
        }

        Success = true;
        return Page();
    }
}
