using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Services.Interfaces;

public interface IPaymentService
{
    Task<List<CreditPackage>> GetActivePackagesAsync();
    Task<PaymentInitResult> InitiatePaymentAsync(string userId, int packageId, string callbackUrl);
    Task<PaymentVerifyResult> VerifyPaymentAsync(string authority);
    Task<List<Payment>> GetPaymentHistoryAsync(string userId, int page = 1, int pageSize = 20);
}

public class PaymentInitResult
{
    public bool IsSuccess { get; set; }
    public string? RedirectUrl { get; set; }
    public string? ErrorMessage { get; set; }

    public static PaymentInitResult Success(string url) => new() { IsSuccess = true, RedirectUrl = url };
    public static PaymentInitResult Fail(string error) => new() { IsSuccess = false, ErrorMessage = error };
}

public class PaymentVerifyResult
{
    public bool IsSuccess { get; set; }
    public string? RefId { get; set; }
    public int CreditAdded { get; set; }
    public string? ErrorMessage { get; set; }
    public bool AlreadyVerified { get; set; }

    public static PaymentVerifyResult Success(string refId, int credit)
        => new() { IsSuccess = true, RefId = refId, CreditAdded = credit };
    public static PaymentVerifyResult AlreadyDone()
        => new() { IsSuccess = true, AlreadyVerified = true };
    public static PaymentVerifyResult Fail(string error)
        => new() { IsSuccess = false, ErrorMessage = error };
}
