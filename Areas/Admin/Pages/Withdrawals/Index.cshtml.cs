using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.Admin.Pages.Withdrawals;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IWithdrawalService _withdrawals;
    private readonly INotificationService _notify;

    public IndexModel(ApplicationDbContext db, IWithdrawalService withdrawals, INotificationService notify)
    {
        _db = db;
        _withdrawals = withdrawals;
        _notify = notify;
    }

    public List<WithdrawalRow> Pending { get; set; } = new();
    public List<WithdrawalRow> Processed { get; set; } = new();
    public int ProcessedTotalCount { get; set; }

    [BindProperty] public string? AdminNote { get; set; }
    [BindProperty] public string? PaymentRef { get; set; }

    [BindProperty(SupportsGet = true)] public WithdrawalStatus? FilterStatus { get; set; }
    [BindProperty(SupportsGet = true)] public int ProcessedPage { get; set; } = 1;
    private const int ProcessedPageSize = 20;

    public async Task OnGetAsync()
    {
        await LoadDataAsync();
    }

    public async Task<IActionResult> OnPostApproveAsync(int id)
    {
        var wr = await _db.WithdrawalRequests.Include(w => w.Creator).FirstOrDefaultAsync(w => w.Id == id);
        var note = string.IsNullOrWhiteSpace(PaymentRef) ? null : $"شناسه پرداخت: {PaymentRef}";
        await _withdrawals.ProcessRequestAsync(id, WithdrawalStatus.Paid, note);
        await AddAuditAsync("ApproveWithdrawal", "WithdrawalRequest", id.ToString(),
            $"درخواست برداشت #{id} تایید شد — {note}");
        if (wr?.Creator?.UserId != null)
            await _notify.CreateAsync(wr.Creator.UserId,
                $"درخواست برداشت شما پرداخت شد",
                note ?? "مبلغ درخواستی به حساب شما واریز شد.",
                "/Creator/Earnings/Index", "withdrawal");
        TempData["Success"] = "درخواست تایید و پرداخت ثبت شد.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(int id)
    {
        var wr = await _db.WithdrawalRequests.Include(w => w.Creator).FirstOrDefaultAsync(w => w.Id == id);
        var reason = string.IsNullOrWhiteSpace(AdminNote) ? "بدون دلیل" : AdminNote;
        await _withdrawals.ProcessRequestAsync(id, WithdrawalStatus.Rejected, reason);
        await AddAuditAsync("RejectWithdrawal", "WithdrawalRequest", id.ToString(),
            $"درخواست برداشت #{id} رد شد — {reason}");
        if (wr?.Creator?.UserId != null)
            await _notify.CreateAsync(wr.Creator.UserId,
                $"درخواست برداشت شما رد شد",
                $"دلیل: {reason}",
                "/Creator/Earnings/Index", "withdrawal");
        TempData["Warning"] = "درخواست رد شد.";
        return RedirectToPage();
    }

    private async Task LoadDataAsync()
    {
        var pendingRaw = await _db.WithdrawalRequests
            .Include(w => w.Creator).ThenInclude(c => c.User)
            .Where(w => w.Status == WithdrawalStatus.Pending)
            .OrderBy(w => w.CreatedAt)
            .ToListAsync();

        Pending = pendingRaw.Select(Map).ToList();

        var processedQuery = _db.WithdrawalRequests
            .Include(w => w.Creator).ThenInclude(c => c.User)
            .Where(w => w.Status != WithdrawalStatus.Pending)
            .AsQueryable();

        if (FilterStatus.HasValue)
            processedQuery = processedQuery.Where(w => w.Status == FilterStatus.Value);

        ProcessedTotalCount = await processedQuery.CountAsync();
        var processedRaw = await processedQuery
            .OrderByDescending(w => w.ProcessedAt)
            .Skip((ProcessedPage - 1) * ProcessedPageSize)
            .Take(ProcessedPageSize)
            .ToListAsync();

        Processed = processedRaw.Select(Map).ToList();
    }

    private static WithdrawalRow Map(WithdrawalRequest w)
    {
        string sheba = "", owner = "";
        try
        {
            var doc = JsonDocument.Parse(w.BankAccountInfo);
            sheba = doc.RootElement.GetProperty("sheba").GetString() ?? "";
            owner = doc.RootElement.GetProperty("accountOwner").GetString() ?? "";
        }
        catch { }
        return new WithdrawalRow(w, sheba, owner);
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

    public record WithdrawalRow(WithdrawalRequest Request, string Sheba, string AccountOwner);
}
