using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Pages.Auth;

public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _email;

    public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailService email)
    {
        _userManager = userManager;
        _email = email;
    }

    [BindProperty]
    [Required(ErrorMessage = "ایمیل الزامی است")]
    [EmailAddress(ErrorMessage = "ایمیل معتبر وارد کنید")]
    public string Email { get; set; } = string.Empty;

    public bool Submitted { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var user = await _userManager.FindByEmailAsync(Email);

        // حتی اگر کاربر وجود نداشته باشد، پیام موفقیت نمایش می‌دهیم (امنیت)
        if (user != null && await _userManager.IsEmailConfirmedAsync(user))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetUrl = Url.Page(
                "/Auth/ResetPassword",
                pageHandler: null,
                values: new { email = Email, token },
                protocol: Request.Scheme)!;

            var body = $"""
                <div dir="rtl" style="font-family:Tahoma;font-size:14px;">
                    <p>سلام {user.DisplayName}،</p>
                    <p>برای بازیابی رمز عبور، روی لینک زیر کلیک کنید:</p>
                    <p><a href="{resetUrl}" style="background:#f97316;color:#fff;padding:10px 20px;text-decoration:none;border-radius:6px;">بازیابی رمز عبور</a></p>
                    <p style="color:#999;font-size:12px;">این لینک ۲ ساعت معتبر است. اگر این درخواست را شما ارسال نکرده‌اید، این ایمیل را نادیده بگیرید.</p>
                </div>
                """;

            await _email.SendAsync(Email, "بازیابی رمز عبور — پرامپت مارکت", body);
        }

        Submitted = true;
        return Page();
    }
}
