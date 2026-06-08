using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Services;

public class WithdrawalService : IWithdrawalService
{
    private readonly ApplicationDbContext _db;
    private readonly ISettingService _settings;

    public WithdrawalService(ApplicationDbContext db, ISettingService settings)
    {
        _db = db;
        _settings = settings;
    }

    public async Task<ExecutionResult> RequestWithdrawalAsync(int creatorProfileId, decimal amount,
        string sheba, string accountOwner)
    {
        var minAmountStr = await _settings.GetValueAsync("Withdrawal:MinimumAmount", "500000");
        var minAmount = decimal.TryParse(minAmountStr, out var parsed) ? parsed : 500000m;

        if (amount < minAmount)
            return ExecutionResult.Fail($"حداقل مبلغ برداشت {minAmount:N0} ریال است.");

        var creator = await _db.CreatorProfiles
            .Include(c => c.User).ThenInclude(u => u.Wallet)
            .FirstOrDefaultAsync(c => c.Id == creatorProfileId);

        if (creator == null)
            return ExecutionResult.Fail("پروفایل سازنده یافت نشد.");

        var wallet = creator.User.Wallet;
        if (wallet == null || wallet.EarningBalance < amount)
            return ExecutionResult.Fail("موجودی کافی برای برداشت وجود ندارد.");

        // بررسی درخواست در انتظار قبلی
        var hasPending = await _db.WithdrawalRequests
            .AnyAsync(w => w.CreatorProfileId == creatorProfileId && w.Status == WithdrawalStatus.Pending);

        if (hasPending)
            return ExecutionResult.Fail("یک درخواست برداشت در انتظار پردازش دارید.");

        var bankInfo = JsonSerializer.Serialize(new { sheba, accountOwner });

        _db.WithdrawalRequests.Add(new WithdrawalRequest
        {
            CreatorProfileId = creatorProfileId,
            Amount = amount,
            BankAccountInfo = bankInfo,
            Status = WithdrawalStatus.Pending
        });

        // رزرو موجودی
        wallet.EarningBalance -= amount;
        await _db.SaveChangesAsync();

        return ExecutionResult.Success(null!);
    }

    public async Task<List<WithdrawalRequest>> GetCreatorRequestsAsync(int creatorProfileId)
        => await _db.WithdrawalRequests
            .Where(w => w.CreatorProfileId == creatorProfileId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();

    public async Task<List<WithdrawalRequest>> GetPendingRequestsAsync()
        => await _db.WithdrawalRequests
            .Include(w => w.Creator).ThenInclude(c => c.User)
            .Where(w => w.Status == WithdrawalStatus.Pending)
            .OrderBy(w => w.CreatedAt)
            .ToListAsync();

    public async Task<ExecutionResult> ProcessRequestAsync(int requestId, WithdrawalStatus newStatus,
        string? adminNote = null)
    {
        var request = await _db.WithdrawalRequests
            .Include(w => w.Creator).ThenInclude(c => c.User).ThenInclude(u => u.Wallet)
            .FirstOrDefaultAsync(w => w.Id == requestId);

        if (request == null)
            return ExecutionResult.Fail("درخواست یافت نشد.");

        if (request.Status != WithdrawalStatus.Pending)
            return ExecutionResult.Fail("این درخواست قبلاً پردازش شده است.");

        request.Status = newStatus;
        request.AdminNote = adminNote;
        request.ProcessedAt = DateTime.UtcNow;

        // اگر رد شد، موجودی را برگردان
        if (newStatus == WithdrawalStatus.Rejected && request.Creator.User.Wallet != null)
        {
            request.Creator.User.Wallet.EarningBalance += request.Amount;
        }

        // اگر پرداخت شد، TotalWithdrawn را بروز کن
        if (newStatus == WithdrawalStatus.Paid && request.Creator.User.Wallet != null)
        {
            request.Creator.User.Wallet.TotalWithdrawn += request.Amount;
        }

        await _db.SaveChangesAsync();
        return ExecutionResult.Success(null!);
    }
}
