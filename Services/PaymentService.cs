using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Services;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _db;
    private readonly ISettingService _settings;
    private readonly ICreditService _credits;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<PaymentService> _logger;
    private readonly INotificationService _notify;

    public PaymentService(ApplicationDbContext db, ISettingService settings, ICreditService credits,
        UserManager<ApplicationUser> userManager, IHttpClientFactory httpFactory,
        ILogger<PaymentService> logger, INotificationService notify)
    {
        _db = db;
        _settings = settings;
        _credits = credits;
        _userManager = userManager;
        _httpFactory = httpFactory;
        _logger = logger;
        _notify = notify;
    }

    public async Task<List<Payment>> GetPaymentHistoryAsync(string userId, int page = 1, int pageSize = 20)
        => await _db.Payments
            .Include(p => p.Package)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<List<CreditPackage>> GetActivePackagesAsync()
        => await _db.CreditPackages
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .ToListAsync();

    public async Task<PaymentInitResult> InitiatePaymentAsync(string userId, int packageId, string callbackUrl)
    {
        var package = await _db.CreditPackages.FindAsync(packageId);
        if (package == null || !package.IsActive)
            return PaymentInitResult.Fail("بسته اعتباری یافت نشد.");

        var config = await _settings.GetZarinPalConfigAsync();
        if (string.IsNullOrWhiteSpace(config.MerchantId))
            return PaymentInitResult.Fail("درگاه پرداخت هنوز پیکربندی نشده است.");

        var user = await _userManager.FindByIdAsync(userId);

        // ساخت رکورد Payment
        var payment = new Payment
        {
            UserId = userId,
            PackageId = packageId,
            Amount = package.PriceRial,
            CreditAmount = package.CreditAmount,
            Status = PaymentStatus.Pending
        };
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        // درخواست به ZarinPal
        var body = JsonSerializer.Serialize(new
        {
            merchant_id = config.MerchantId,
            amount = (long)package.PriceRial,
            description = $"{config.Description} — {package.CreditAmount} اعتبار",
            callback_url = callbackUrl,
            metadata = new
            {
                mobile = user?.PhoneNumber ?? "",
                email = user?.Email ?? ""
            }
        });

        try
        {
            var client = _httpFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            var request = new HttpRequestMessage(HttpMethod.Post, config.RequestUrl)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            var node = JsonNode.Parse(json);

            var code = node?["data"]?["code"]?.GetValue<int>();
            var authority = node?["data"]?["authority"]?.GetValue<string>();

            if (code != 100 || string.IsNullOrEmpty(authority))
            {
                var errMessage = node?["errors"]?.ToString() ?? json;
                _logger.LogWarning("ZarinPal request failed. Code:{Code} Response:{Response}", code, errMessage);
                payment.Status = PaymentStatus.Failed;
                await _db.SaveChangesAsync();
                return PaymentInitResult.Fail("خطا در اتصال به درگاه پرداخت.");
            }

            payment.ZarinpalAuthority = authority;
            await _db.SaveChangesAsync();

            return PaymentInitResult.Success(config.GatewayUrl(authority));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ZarinPal request exception for payment {PaymentId}", payment.Id);
            payment.Status = PaymentStatus.Failed;
            await _db.SaveChangesAsync();
            return PaymentInitResult.Fail("خطا در ارتباط با درگاه پرداخت.");
        }
    }

    public async Task<PaymentVerifyResult> VerifyPaymentAsync(string authority)
    {
        var payment = await _db.Payments
            .Include(p => p.Package)
            .FirstOrDefaultAsync(p => p.ZarinpalAuthority == authority);

        if (payment == null)
            return PaymentVerifyResult.Fail("تراکنش یافت نشد.");

        // جلوگیری از Verify دوباره
        if (payment.Status == PaymentStatus.Verified)
            return PaymentVerifyResult.AlreadyDone();

        if (payment.Status == PaymentStatus.Failed)
            return PaymentVerifyResult.Fail("این تراکنش قبلاً ناموفق ثبت شده است.");

        var config = await _settings.GetZarinPalConfigAsync();

        var body = JsonSerializer.Serialize(new
        {
            merchant_id = config.MerchantId,
            amount = (long)payment.Amount,
            authority
        });

        try
        {
            var client = _httpFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            var request = new HttpRequestMessage(HttpMethod.Post, config.VerifyUrl)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            var node = JsonNode.Parse(json);

            var code = node?["data"]?["code"]?.GetValue<int>();
            var refId = node?["data"]?["ref_id"]?.GetValue<long>().ToString();

            // 100 = پرداخت شده، 101 = قبلاً تایید شده
            if (code != 100 && code != 101)
            {
                _logger.LogWarning("ZarinPal verify failed. Code:{Code} Authority:{Auth}", code, authority);
                payment.Status = PaymentStatus.Failed;
                await _db.SaveChangesAsync();
                return PaymentVerifyResult.Fail("تأیید پرداخت از درگاه ناموفق بود.");
            }

            if (code == 101)
            {
                payment.Status = PaymentStatus.Verified;
                await _db.SaveChangesAsync();
                return PaymentVerifyResult.AlreadyDone();
            }

            // ─── پرداخت موفق ─────────────────────────────────────
            payment.Status = PaymentStatus.Verified;
            payment.ZarinpalRefId = refId;
            payment.VerifiedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // افزودن اعتبار به کیف‌پول
            await _credits.AddCreditsAsync(
                payment.UserId,
                payment.CreditAmount,
                $"خرید {payment.CreditAmount} اعتبار — کد پیگیری: {refId}",
                payment.Id.ToString(),
                TransactionType.Purchase);

            await _notify.CreateAsync(payment.UserId,
                $"خرید اعتبار موفق: {payment.CreditAmount} اعتبار",
                $"پرداخت شما با موفقیت تایید شد. {payment.CreditAmount} اعتبار به کیف‌پول شما اضافه شد. کد پیگیری: {refId}",
                null, "general");

            return PaymentVerifyResult.Success(refId ?? "", payment.CreditAmount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ZarinPal verify exception for authority {Authority}", authority);
            return PaymentVerifyResult.Fail("خطا در تأیید پرداخت. لطفاً با پشتیبانی تماس بگیرید.");
        }
    }
}
