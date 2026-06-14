using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.Creator.Pages.Earnings;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IWithdrawalService _withdrawal;
    private readonly ICreatorHelper _ch;
    private readonly INotificationService _notify;

    public IndexModel(ApplicationDbContext db, IWithdrawalService withdrawal, ICreatorHelper ch, INotificationService notify)
    { _db = db; _withdrawal = withdrawal; _ch = ch; _notify = notify; }

    public UserWallet? Wallet { get; set; }
    public List<WalletTransaction> EarnTransactions { get; set; } = new();
    public List<WithdrawalRequest> WithdrawalRequests { get; set; } = new();
    public int CreatorProfileId { get; set; }

    [BindProperty] public WithdrawalForm Form { get; set; } = new();

    public async Task OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var cid = await _ch.GetCreatorProfileIdAsync(User);
        CreatorProfileId = cid ?? 0;

        Wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);

        EarnTransactions = await _db.WalletTransactions
            .Where(t => t.UserId == userId && t.Type == TransactionType.Earn)
            .OrderByDescending(t => t.CreatedAt)
            .Take(30)
            .ToListAsync();

        if (cid.HasValue)
            WithdrawalRequests = await _withdrawal.GetCreatorRequestsAsync(cid.Value);
    }

    public async Task<IActionResult> OnPostWithdrawAsync()
    {
        if (!ModelState.IsValid) { await OnGetAsync(); return Page(); }

        var cid = await _ch.GetCreatorProfileIdAsync(User);
        if (cid == null) return Forbid();

        var result = await _withdrawal.RequestWithdrawalAsync(cid.Value, Form.Amount, Form.Sheba, Form.AccountOwner);
        if (result.IsSuccess)
        {
            await _notify.CreateForAdminsAsync(
                $"درخواست برداشت جدید: {Form.Amount:N0} تومان",
                $"شماره شبا: {Form.Sheba} — به نام: {Form.AccountOwner}",
                "/Admin/Withdrawals/Index",
                "withdrawal");
        }
        TempData[result.IsSuccess ? "Success" : "Error"] =
            result.IsSuccess ? "درخواست برداشت ثبت شد." : result.ErrorMessage;

        return RedirectToPage();
    }

    public class WithdrawalForm
    {
        [Range(1, 1000000000)] public decimal Amount { get; set; }
        [Required][MinLength(24)][MaxLength(26)] public string Sheba { get; set; } = string.Empty;
        [Required] public string AccountOwner { get; set; } = string.Empty;
    }
}
