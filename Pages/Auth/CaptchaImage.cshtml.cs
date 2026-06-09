using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PromptMarketPlace.Services;

namespace PromptMarketPlace.Pages.Auth;

public class CaptchaImageModel : PageModel
{
    private readonly ICaptchaService _captcha;

    public CaptchaImageModel(ICaptchaService captcha) => _captcha = captcha;

    public IActionResult OnGet()
    {
        Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
        Response.Headers["Pragma"] = "no-cache";

        var code = _captcha.GenerateCode();
        _captcha.StoreCode(HttpContext.Session, code);
        return File(_captcha.GenerateImage(code), "image/svg+xml");
    }
}
