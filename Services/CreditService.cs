using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Services;

public class CreditService : ICreditService
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;

    public CreditService(ApplicationDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<int> GetBalanceAsync(string userId)
    {
        var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        return wallet?.CreditBalance ?? 0;
    }

    public async Task<bool> HasEnoughCreditsAsync(string userId, int required)
        => await GetBalanceAsync(userId) >= required;

    public async Task EnsureWalletExistsAsync(string userId)
    {
        var exists = await _db.Wallets.AnyAsync(w => w.UserId == userId);
        if (exists) return;
        try
        {
            _db.Wallets.Add(new UserWallet { UserId = userId });
            await _db.SaveChangesAsync();
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            // race condition: another request already created the wallet
            _db.ChangeTracker.Clear();
        }
    }

    public async Task DeductCreditsAsync(string userId, int amount, string description, string? referenceId = null)
    {
        var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId)
            ?? throw new InvalidOperationException("Wallet not found.");

        if (wallet.CreditBalance < amount)
            throw new InvalidOperationException("موجودی کافی نیست.");

        wallet.CreditBalance -= amount;

        _db.WalletTransactions.Add(new WalletTransaction
        {
            UserId = userId,
            Type = TransactionType.Spend,
            CreditAmount = -amount,
            Description = description,
            ReferenceId = referenceId
        });

        await _db.SaveChangesAsync();
    }

    public async Task AddCreditsAsync(string userId, int amount, string description,
        string? referenceId = null, TransactionType type = TransactionType.Purchase)
    {
        await EnsureWalletExistsAsync(userId);
        var wallet = await _db.Wallets.FirstAsync(w => w.UserId == userId);
        wallet.CreditBalance += amount;

        _db.WalletTransactions.Add(new WalletTransaction
        {
            UserId = userId,
            Type = type,
            CreditAmount = amount,
            Description = description,
            ReferenceId = referenceId
        });

        await _db.SaveChangesAsync();
    }

    public async Task DistributeEarningsAsync(AppExecution execution, AiApp app)
    {
        var creator = await _db.CreatorProfiles
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == app.CreatorProfileId);

        if (creator == null) return;

        var creditValueRial = await GetCreditValueRialAsync();
        var totalRial = execution.CreditUsed * creditValueRial;

        var creatorPercent = creator.CommissionPercent;
        var creatorEarning = Math.Round(totalRial * creatorPercent / 100, 2);

        var creatorWallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == creator.UserId);
        if (creatorWallet == null) return;

        creatorWallet.EarningBalance += creatorEarning;
        creatorWallet.TotalEarned += creatorEarning;

        _db.WalletTransactions.Add(new WalletTransaction
        {
            UserId = creator.UserId,
            Type = TransactionType.Earn,
            MoneyAmount = creatorEarning,
            Description = $"درآمد از اجرای ابزار: {app.Title}",
            ReferenceId = execution.Id.ToString()
        });

        await _db.SaveChangesAsync();
    }

    public async Task<List<WalletTransaction>> GetTransactionHistoryAsync(string userId,
        int page = 1, int pageSize = 20)
        => await _db.WalletTransactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    private async Task<decimal> GetCreditValueRialAsync()
    {
        var cheapest = await _db.CreditPackages
            .Where(p => p.IsActive && p.CreditAmount > 0)
            .OrderBy(p => p.PriceRial / p.CreditAmount)
            .FirstOrDefaultAsync();

        if (cheapest != null)
            return cheapest.PriceRial / cheapest.CreditAmount;

        return _config.GetValue<decimal>("Pricing:FallbackCreditValueRial", 5000m);
    }
}
