using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;
using PromptMarketPlace.Services;

namespace PromptMarketPlace.Pages.Auth;

public class RegisterModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly ApplicationDbContext _db;
    private readonly ICaptchaService _captcha;

    public RegisterModel(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signIn, ApplicationDbContext db, ICaptchaService captcha)
    {
        _userManager = userManager;
        _signIn = signIn;
        _db = db;
        _captcha = captcha;
    }

    [BindProperty] public RegisterForm Form { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        if (!_captcha.Validate(HttpContext.Session, Form.CaptchaInput))
        {
            ModelState.AddModelError("", "کد امنیتی اشتباه است. لطفاً دوباره تلاش کنید.");
            return Page();
        }

        var user = new ApplicationUser
        {
            UserName = Form.Email,
            Email = Form.Email,
            DisplayName = Form.DisplayName,
            EmailConfirmed = true,
            Role = Form.WantToBeCreator ? UserRole.Creator : UserRole.User
        };

        var result = await _userManager.CreateAsync(user, Form.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError("", e.Description);
            return Page();
        }

        await _userManager.AddToRoleAsync(user, "User");
        _db.Wallets.Add(new UserWallet { UserId = user.Id });

        if (Form.WantToBeCreator)
        {
            await _userManager.AddToRoleAsync(user, "Creator");
            _db.CreatorProfiles.Add(new CreatorProfile
            {
                UserId = user.Id,
                CommissionPercent = 70,
                JoinedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
        await _signIn.SignInAsync(user, isPersistent: false);
        return RedirectToPage("/Index");
    }

    public class RegisterForm
    {
        [Required(ErrorMessage = "نام نمایشی الزامی است")]
        [MaxLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        [Required(ErrorMessage = "ایمیل الزامی است")]
        [EmailAddress(ErrorMessage = "ایمیل معتبر وارد کنید")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "رمز عبور الزامی است")]
        [MinLength(8, ErrorMessage = "رمز عبور حداقل ۸ کاراکتر باشد")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "رمزهای عبور مطابقت ندارند")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        public bool WantToBeCreator { get; set; }

        [Required(ErrorMessage = "کد امنیتی الزامی است")]
        public string CaptchaInput { get; set; } = string.Empty;
    }
}
