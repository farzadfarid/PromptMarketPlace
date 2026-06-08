using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoginModel(SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> userManager)
    {
        _signIn = signIn;
        _userManager = userManager;
    }

    [BindProperty] public LoginForm Form { get; set; } = new();
    public string? ReturnUrl { get; set; }

    public void OnGet(string? returnUrl = null) => ReturnUrl = returnUrl;

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!ModelState.IsValid) return Page();

        var result = await _signIn.PasswordSignInAsync(Form.Email, Form.Password,
            Form.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            var user = await _userManager.FindByEmailAsync(Form.Email);
            if (user != null)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                    return RedirectToPage("/Dashboard/Index", new { area = "Admin" });
                if (await _userManager.IsInRoleAsync(user, "Creator"))
                    return RedirectToPage("/Dashboard/Index", new { area = "Creator" });
            }
            return RedirectToPage("/Dashboard/Index", new { area = "User" });
        }

        if (result.IsLockedOut)
            ModelState.AddModelError("", "حساب کاربری موقتاً قفل شده است.");
        else
            ModelState.AddModelError("", "ایمیل یا رمز عبور اشتباه است.");

        return Page();
    }

    public class LoginForm
    {
        [Required(ErrorMessage = "ایمیل الزامی است")]
        [EmailAddress(ErrorMessage = "ایمیل معتبر وارد کنید")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "رمز عبور الزامی است")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}
