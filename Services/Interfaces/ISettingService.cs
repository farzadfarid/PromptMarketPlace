namespace PromptMarketPlace.Services.Interfaces;

public interface ISettingService
{
    Task<string?> GetValueAsync(string key);
    Task<string> GetValueAsync(string key, string defaultValue);
    Task SetValueAsync(string key, string value);
    Task<Dictionary<string, string>> GetGroupAsync(string group);
    Task<ZarinPalConfig> GetZarinPalConfigAsync();
}

public class ZarinPalConfig
{
    public string MerchantId { get; set; } = string.Empty;
    public bool IsSandbox { get; set; } = true;
    public string Description { get; set; } = "خرید اعتبار";

    public string RequestUrl => IsSandbox
        ? "https://sandbox.zarinpal.com/pg/v4/payment/request.json"
        : "https://payment.zarinpal.com/pg/v4/payment/request.json";

    public string VerifyUrl => IsSandbox
        ? "https://sandbox.zarinpal.com/pg/v4/payment/verify.json"
        : "https://payment.zarinpal.com/pg/v4/payment/verify.json";

    public string GatewayUrl(string authority) => IsSandbox
        ? $"https://sandbox.zarinpal.com/pg/StartPay/{authority}"
        : $"https://www.zarinpal.com/pg/StartPay/{authority}";
}
