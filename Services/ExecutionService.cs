using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Helpers;
using PromptMarketPlace.Models;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Services;

public class ExecutionService : IExecutionService
{
    private readonly ApplicationDbContext _db;
    private readonly ICreditService _credits;
    private readonly IAiService _ai;
    private readonly IAiProviderService _providers;
    private readonly IEncryptionService _encryption;
    private readonly IStorageService _storage;
    private readonly IConfiguration _config;
    private readonly ILogger<ExecutionService> _logger;

    public ExecutionService(ApplicationDbContext db, ICreditService credits, IAiService ai,
        IAiProviderService providers, IEncryptionService encryption, IStorageService storage,
        IConfiguration config, ILogger<ExecutionService> logger)
    {
        _db = db;
        _credits = credits;
        _ai = ai;
        _providers = providers;
        _encryption = encryption;
        _storage = storage;
        _config = config;
        _logger = logger;
    }

    public async Task<ExecutionResult> ExecuteAsync(string userId, int appId, Dictionary<string, string> inputs, List<string>? inputImageUrls = null)
    {
        // ─── ۱. بارگذاری ابزار ────────────────────────────────────
        var app = await _db.Apps
            .Include(a => a.InputFields)
            .Include(a => a.AiModel).ThenInclude(m => m.Provider)
            .Include(a => a.Creator)
            .FirstOrDefaultAsync(a => a.Id == appId && a.Status == AppStatus.Active);

        if (app == null)
            return ExecutionResult.Fail("ابزار یافت نشد یا غیرفعال است.");

        // ─── ۲. سازنده نمی‌تواند ابزار خودش را اجرا کند ──────────
        if (app.Creator.UserId == userId)
            return ExecutionResult.Fail("سازنده نمی‌تواند ابزار خودش را اجرا کند.");

        // ─── ۳. Rate Limiting ─────────────────────────────────────
        var rateLimitError = await CheckRateLimitAsync(userId);
        if (rateLimitError != null)
            return ExecutionResult.Fail(rateLimitError);

        // ─── ۴. بررسی موجودی اعتبار ──────────────────────────────
        if (!await _credits.HasEnoughCreditsAsync(userId, app.CreditCost))
            return ExecutionResult.Fail("موجودی اعتبار کافی نیست. لطفاً اعتبار خود را شارژ کنید.");

        // ─── ۵. اعتبارسنجی ورودی‌ها ──────────────────────────────
        var validationError = InputValidator.Validate(app.InputFields.ToList(), inputs);
        if (validationError != null)
            return ExecutionResult.Fail(validationError);

        // ─── ۶. ساخت رکورد AppExecution (Pending) ────────────────
        var execution = new AppExecution
        {
            AppId = appId,
            UserId = userId,
            Status = ExecutionStatus.Pending,
            OutputType = app.OutputType,
            CreditUsed = app.CreditCost
        };

        foreach (var kv in inputs)
            execution.InputValues.Add(new ExecutionInputValue { FieldName = kv.Key, FieldValue = kv.Value });

        _db.Executions.Add(execution);
        await _db.SaveChangesAsync();

        var startTime = DateTime.UtcNow;

        // ─── رمزگشایی پرامپت پیش از transaction (فقط در RAM) ────
        var decryptedPrompt = _encryption.Decrypt(app.EncryptedPrompt);
        var finalPrompt = InputValidator.SubstituteVariables(decryptedPrompt, inputs);

        // ─── پیدا کردن provider و مدل فعال برای این نوع خروجی ────
        var (activeProvider, activeModel, apiKey) =
            await _providers.GetActiveSetupForOutputTypeAsync(app.OutputType);

        if (activeProvider == null)
        {
            await FailExecutionAsync(execution, "سرویس‌دهنده‌ای برای این نوع ابزار فعال نشده.");
            // اعتبار کسر نشده، بازگشت لازم نیست
            return ExecutionResult.Fail("سرویس‌دهنده هوش مصنوعی برای این نوع ابزار پیکربندی نشده. لطفاً با ادمین تماس بگیرید.");
        }

        if (activeModel == null)
        {
            await FailExecutionAsync(execution, "مدل پیش‌فرضی برای این نوع خروجی روی سرویس‌دهنده فعال یافت نشد.");
            // اعتبار کسر نشده، بازگشت لازم نیست
            return ExecutionResult.Fail("مدل هوش مصنوعی برای این نوع ابزار پیکربندی نشده. لطفاً با ادمین تماس بگیرید.");
        }

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // ─── ۷. کسر اعتبار ────────────────────────────────────
            await _credits.DeductCreditsAsync(userId, app.CreditCost,
                $"اجرای ابزار: {app.Title}", execution.Id.ToString());

            // ─── ۸. تغییر وضعیت به Running ────────────────────────
            execution.Status = ExecutionStatus.Running;
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Transaction failed before AI call for execution {Id}", execution.Id);
            await FailExecutionAsync(execution, "خطا در پردازش تراکنش.");
            return ExecutionResult.Fail("خطا در پردازش. لطفاً مجدد تلاش کنید.");
        }

        try
        {
            // ─── ۹. فراخوانی AI (خارج از transaction چون کند است) ─
            var aiResponse = await _ai.RunAsync(activeModel, apiKey, app.SystemContext,
                finalPrompt, app.OutputType, inputImageUrls);

            if (!aiResponse.IsSuccess)
            {
                await FailExecutionAsync(execution, aiResponse.ErrorMessage ?? "خطای ناشناخته");
                await RefundCreditAsync(userId, app.CreditCost, execution.Id);
                return ExecutionResult.Fail(aiResponse.ErrorMessage ?? "خطا در اجرای هوش مصنوعی.");
            }

            // ─── ۱۰. پردازش خروجی داینامیک ───────────────────────
            await ProcessOutputAsync(execution, aiResponse, app.OutputType, apiKey);

            execution.Status = ExecutionStatus.Completed;
            execution.TokensUsed = aiResponse.TokensUsed > 0 ? aiResponse.TokensUsed : (int?)null;
            execution.ActualApiCost = aiResponse.ActualCost;
            execution.Duration = DateTime.UtcNow - startTime;
            await _db.SaveChangesAsync();

            // ─── ۱۱. توزیع درآمد ──────────────────────────────────
            await _credits.DistributeEarningsAsync(execution, app);

            // ─── ۱۲. افزایش تعداد اجرا ────────────────────────────
            await _db.Apps
                .Where(a => a.Id == appId)
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.ExecutionCount, a => a.ExecutionCount + 1));

            return ExecutionResult.Success(execution);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Execution {ExecutionId} failed after AI call", execution.Id);
            await FailExecutionAsync(execution, "خطای داخلی سرور.");
            await RefundCreditAsync(userId, app.CreditCost, execution.Id);
            return ExecutionResult.Fail("خطای داخلی سرور رخ داد. اعتبار شما بازگشت داده شد.");
        }
    }

    public async Task<AppExecution?> GetExecutionAsync(long id, string userId)
        => await _db.Executions
            .Include(e => e.App)
            .Include(e => e.InputValues)
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

    public async Task<List<AppExecution>> GetUserExecutionsAsync(string userId, int page = 1, int pageSize = 20)
        => await _db.Executions
            .Include(e => e.App)
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<List<AppExecution>> GetAppExecutionsAsync(int appId, int creatorProfileId,
        int page = 1, int pageSize = 20)
    {
        var app = await _db.Apps.FirstOrDefaultAsync(a => a.Id == appId && a.CreatorProfileId == creatorProfileId);
        if (app == null) return new();

        return await _db.Executions
            .Where(e => e.AppId == appId)
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<ExecutionResult> RefundExecutionAsync(long id, string adminUserId)
    {
        var execution = await _db.Executions.Include(e => e.App).FirstOrDefaultAsync(e => e.Id == id);
        if (execution == null) return ExecutionResult.Fail("اجرا یافت نشد.");
        if (execution.Status == ExecutionStatus.Refunded) return ExecutionResult.Fail("اعتبار قبلاً بازگشت داده شده.");

        await _credits.AddCreditsAsync(execution.UserId, execution.CreditUsed,
            $"بازگشت اعتبار توسط ادمین - اجرا #{execution.Id}",
            execution.Id.ToString(), TransactionType.Refund);

        execution.Status = ExecutionStatus.Refunded;
        await _db.SaveChangesAsync();

        return ExecutionResult.Success(execution);
    }

    // ─── Private Helpers ─────────────────────────────────────────────

    private async Task ProcessOutputAsync(AppExecution execution, AiResponse aiResponse,
        OutputType outputType, string? apiKey = null)
    {
        execution.OutputText = aiResponse.Text;

        if (!string.IsNullOrEmpty(aiResponse.ImageUrl))
        {
            try
            {
                // API Key را پاس می‌دهیم تا تصاویر authenticated از ChatQT هم دانلود شوند
                var localPath = await _storage.SaveFromUrlAsync(aiResponse.ImageUrl, "images", apiKey);
                execution.OutputImageUrl = localPath;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not download image from {Url}", aiResponse.ImageUrl);
                // URL خارجی را ذخیره نمی‌کنیم — فقط لاگ می‌زنیم
                execution.OutputImageUrl = null;
            }
        }

        if (!string.IsNullOrEmpty(aiResponse.VideoUrl))
            execution.OutputVideoUrl = aiResponse.VideoUrl;

        if (!string.IsNullOrEmpty(aiResponse.AudioUrl))
        {
            try
            {
                var localPath = await _storage.SaveFromUrlAsync(aiResponse.AudioUrl, "audio");
                execution.OutputAudioUrl = localPath;
            }
            catch
            {
                execution.OutputAudioUrl = aiResponse.AudioUrl;
            }
        }

        // OutputType.Form: AI returns JSON schema as text
        if (outputType == OutputType.Form && !string.IsNullOrEmpty(aiResponse.Text))
            execution.OutputFormSchema = aiResponse.Text;
    }

    private async Task FailExecutionAsync(AppExecution execution, string errorMessage)
    {
        try
        {
            execution.Status = ExecutionStatus.Failed;
            execution.ErrorMessage = errorMessage;
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not save failed status for execution {Id}", execution.Id);
        }
    }

    private async Task RefundCreditAsync(string userId, int amount, long executionId)
    {
        try
        {
            await _credits.AddCreditsAsync(userId, amount,
                "بازگشت اعتبار - خطا در اجرا", executionId.ToString(), TransactionType.Refund);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not refund credits for user {UserId}, execution {Id}", userId, executionId);
        }
    }

    private async Task<string?> CheckRateLimitAsync(string userId)
    {
        var maxPerMinute = _config.GetValue<int>("RateLimiting:MaxExecutionsPerMinute", 5);
        var maxPerDay = _config.GetValue<int>("RateLimiting:MaxExecutionsPerDay", 100);

        var since1Min = DateTime.UtcNow.AddMinutes(-1);
        var since1Day = DateTime.UtcNow.AddDays(-1);

        var countMin = await _db.Executions
            .CountAsync(e => e.UserId == userId && e.CreatedAt >= since1Min);

        if (countMin >= maxPerMinute)
            return $"بیش از {maxPerMinute} اجرا در یک دقیقه مجاز نیست. لطفاً کمی صبر کنید.";

        var countDay = await _db.Executions
            .CountAsync(e => e.UserId == userId && e.CreatedAt >= since1Day);

        if (countDay >= maxPerDay)
            return $"سقف روزانه ({maxPerDay} اجرا) به پایان رسیده است.";

        return null;
    }
}
