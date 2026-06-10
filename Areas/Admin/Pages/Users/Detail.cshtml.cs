using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.Admin.Pages.Users;

public class DetailModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICreditService _credits;

    public DetailModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager, ICreditService credits)
    {
        _db = db;
        _userManager = userManager;
        _credits = credits;
    }

    public ApplicationUser? TargetUser { get; set; }
    public int CreditBalance { get; set; }
    public List<AppExecution> RecentExecutions { get; set; } = new();
    public List<Payment> RecentPayments { get; set; } = new();
    public int ExecutionTotalCount { get; set; }

    [BindProperty(SupportsGet = true)] public ExecutionStatus? FilterStatus { get; set; }
    [BindProperty(SupportsGet = true)] public int ExecPage { get; set; } = 1;
    private const int ExecPageSize = 15;

    [BindProperty] public int CreditAdjustment { get; set; }
    [BindProperty] public string CreditReason { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(string id)
    {
        TargetUser = await _db.Users.FindAsync(id);
        if (TargetUser == null) return NotFound();
        await LoadDataAsync(id);
        return Page();
    }

    public async Task<IActionResult> OnPostToggleBlockAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);

        // باطل کردن تمام session‌های فعال کاربر
        if (!user.IsActive)
            await _userManager.UpdateSecurityStampAsync(user);

        await AddAuditAsync(user.IsActive ? "UnblockUser" : "BlockUser", "User", id,
            $"کاربر {user.Email} {(user.IsActive ? "فعال" : "مسدود")} شد");

        TempData["Success"] = user.IsActive ? "کاربر فعال شد." : "کاربر مسدود شد.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostChangeRoleAsync(string id, string newRole)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        if (!Enum.TryParse<UserRole>(newRole, out var role)) return BadRequest();

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, newRole);
        user.Role = role;
        await _userManager.UpdateAsync(user);

        await AddAuditAsync("ChangeRole", "User", id, $"نقش {user.Email} به {newRole} تغییر یافت");

        TempData["Success"] = $"نقش کاربر به {newRole} تغییر یافت.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostAdjustCreditAsync(string id)
    {
        if (CreditAdjustment == 0 || string.IsNullOrWhiteSpace(CreditReason))
        {
            TempData["Error"] = "مقدار و دلیل الزامی است.";
            return RedirectToPage(new { id });
        }

        await _credits.EnsureWalletExistsAsync(id);

        if (CreditAdjustment > 0)
            await _credits.AddCreditsAsync(id, CreditAdjustment, CreditReason, null, TransactionType.AdminAdjust);
        else
            await _credits.DeductCreditsAsync(id, Math.Abs(CreditAdjustment), CreditReason);

        await AddAuditAsync("AdjustCredit", "User", id,
            $"تعدیل اعتبار {CreditAdjustment:+#;-#;0} — {CreditReason}");

        TempData["Success"] = "اعتبار تنظیم شد.";
        return RedirectToPage(new { id });
    }

    private async Task LoadDataAsync(string userId)
    {
        CreditBalance = await _credits.GetBalanceAsync(userId);

        var execQuery = _db.Executions
            .Include(e => e.App)
            .Where(e => e.UserId == userId)
            .AsQueryable();

        if (FilterStatus.HasValue)
            execQuery = execQuery.Where(e => e.Status == FilterStatus.Value);

        ExecutionTotalCount = await execQuery.CountAsync();
        RecentExecutions = await execQuery
            .OrderByDescending(e => e.CreatedAt)
            .Skip((ExecPage - 1) * ExecPageSize)
            .Take(ExecPageSize)
            .ToListAsync();

        RecentPayments = await _db.Payments
            .Include(p => p.Package)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(10)
            .ToListAsync();
    }

    private async Task AddAuditAsync(string action, string targetType, string targetId, string details)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        _db.AuditLogs.Add(new AdminAuditLog
        {
            AdminUserId = adminId,
            Action = action,
            TargetType = targetType,
            TargetId = targetId,
            Details = details,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        });
        await _db.SaveChangesAsync();
    }
}
