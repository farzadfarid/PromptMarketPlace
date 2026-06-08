using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;

namespace PromptMarketPlace.Areas.User.Pages.BecomeCreator;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _config;

    public IndexModel(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration config)
    {
        _db = db;
        _userManager = userManager;
        _signInManager = signInManager;
        _config = config;
    }

    [BindProperty] public string Experience { get; set; } = string.Empty;
    [BindProperty] public string ToolTypes { get; set; } = string.Empty;

    public int ActiveCreatorsCount { get; set; }
    public decimal AverageMonthlyEarning { get; set; }
    public int DefaultCommissionPercent { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (User.IsInRole("Creator"))
            return RedirectToPage("/Dashboard/Index", new { area = "Creator" });

        await LoadStatsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Experience))
            ModelState.AddModelError(nameof(Experience), "Ù„Ø·ÙØ§Ù‹ ØªÙˆØ¶ÛŒØ­ Ù…Ø®ØªØµØ±ÛŒ Ø§Ø² ØªØ¬Ø±Ø¨Ù‡ Ø®ÙˆØ¯ Ø¨Ù†ÙˆÛŒØ³ÛŒØ¯.");
        if (string.IsNullOrWhiteSpace(ToolTypes))
            ModelState.AddModelError(nameof(ToolTypes), "Ù„Ø·ÙØ§Ù‹ Ù†ÙˆØ¹ Ø§Ø¨Ø²Ø§Ø±Ù‡Ø§ÛŒÛŒ Ú©Ù‡ Ù‚ØµØ¯ Ø³Ø§Ø®Øª Ø¯Ø§Ø±ÛŒØ¯ Ø±Ø§ Ù…Ø´Ø®Øµ Ú©Ù†ÛŒØ¯.");

        if (!ModelState.IsValid)
        {
            await LoadStatsAsync();
            return Page();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (await _userManager.IsInRoleAsync(user, "Creator"))
            return RedirectToPage("/Dashboard/Index", new { area = "Creator" });

        var autoApprove = _config.GetValue("Creator:AutoApprove", defaultValue: true);

        if (autoApprove)
        {
            user.Role = UserRole.Creator;
            await _userManager.UpdateAsync(user);

            if (!await _db.CreatorProfiles.AnyAsync(c => c.UserId == user.Id))
            {
                _db.CreatorProfiles.Add(new CreatorProfile
                {
                    UserId = user.Id,
                    Bio = Experience.Trim(),
                    CommissionPercent = _config.GetValue("Credits:DefaultCreatorCommissionPercent", defaultValue: 70m)
                });
                await _db.SaveChangesAsync();
            }

            await _userManager.AddToRoleAsync(user, "Creator");
            await _signInManager.RefreshSignInAsync(user);

            TempData["CreatorWelcome"] = "true";
            return RedirectToPage("/Dashboard/Index", new { area = "Creator" });
        }
        else
        {
            TempData["Pending"] = "Ø¯Ø±Ø®ÙˆØ§Ø³Øª Ø´Ù…Ø§ Ø«Ø¨Øª Ø´Ø¯. Ù¾Ø³ Ø§Ø² Ø¨Ø±Ø±Ø³ÛŒ ØªÙˆØ³Ø· ØªÛŒÙ… Ù…Ø§ØŒ Ø­Ø³Ø§Ø¨ Ø³Ø§Ø²Ù†Ø¯Ù‡ Ø´Ù…Ø§ ÙØ¹Ø§Ù„ Ù…ÛŒâ€ŒØ´ÙˆØ¯.";
            return RedirectToPage();
        }
    }

    private async Task LoadStatsAsync()
    {
        DefaultCommissionPercent = _config.GetValue("Credits:DefaultCreatorCommissionPercent", defaultValue: 70);

        ActiveCreatorsCount = await _db.CreatorProfiles
            .CountAsync(c => c.User.IsActive);

        var earnings = await _db.CreatorProfiles
            .Include(c => c.User).ThenInclude(u => u.Wallet)
            .Where(c => c.User.IsActive && c.User.Wallet != null)
            .Select(c => c.User.Wallet!.TotalEarned)
            .ToListAsync();

        AverageMonthlyEarning = earnings.Any() ? Math.Round(earnings.Average()) : 0;
    }
}

