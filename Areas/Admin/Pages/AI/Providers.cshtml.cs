using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Services.Interfaces;
using PromptMarketPlace.ViewModels.Admin;

namespace PromptMarketPlace.Areas.Admin.Pages.AI;

public class ProvidersModel : PageModel
{
    private readonly IAiProviderService _providerService;
    private readonly IHttpClientFactory _httpFactory;

    public ProvidersModel(IAiProviderService providerService, IHttpClientFactory httpFactory)
    {
        _providerService = providerService;
        _httpFactory = httpFactory;
    }

    public List<AiProvider> Providers { get; set; } = new();

    [BindProperty]
    public AiProviderFormViewModel Form { get; set; } = new();

    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
        => Providers = await _providerService.GetAllProvidersAsync();

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (!ModelState.IsValid)
        {
            Providers = await _providerService.GetAllProvidersAsync();
            return Page();
        }

        if (Form.Id == 0)
            await _providerService.CreateProviderAsync(Form.Name, Form.BaseUrl, Form.ApiKey, Form.Description,
                Form.ProviderType, Form.BalanceUrl, Form.BalanceJsonPath, Form.BalanceCurrency);
        else
            await _providerService.UpdateProviderAsync(Form.Id, Form.Name, Form.BaseUrl, Form.ApiKey, Form.Description,
                Form.ProviderType, Form.BalanceUrl, Form.BalanceJsonPath, Form.BalanceCurrency);

        TempData["Success"] = Form.Id == 0 ? "سرویس‌دهنده با موفقیت افزوده شد." : "سرویس‌دهنده بروزرسانی شد.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        await _providerService.ToggleProviderActiveAsync(id);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            await _providerService.DeleteProviderAsync(id);
            TempData["Success"] = "سرویس‌دهنده حذف شد.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostTestConnectionAsync([FromBody] TestConnectionDto dto)
    {
        var baseUrl = dto.BaseUrl?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
            return new JsonResult(new { success = false, message = "آدرس API وارد نشده." });

        string? apiKey = dto.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey) && dto.ProviderId.HasValue)
            apiKey = await _providerService.GetDecryptedApiKeyAsync(dto.ProviderId.Value);

        try
        {
            // Vertex AI: test by refreshing OAuth token
            if (baseUrl.Contains("aiplatform.googleapis.com", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(apiKey))
                    return new JsonResult(new { success = false, message = "API Key (JSON credentials) تنظیم نشده." });
                try
                {
                    var doc = System.Text.Json.JsonDocument.Parse(apiKey);
                    var clientId     = doc.RootElement.GetProperty("client_id").GetString() ?? "";
                    var clientSecret = doc.RootElement.GetProperty("client_secret").GetString() ?? "";
                    var refreshToken = doc.RootElement.GetProperty("refresh_token").GetString() ?? "";

                    var tokenClient = _httpFactory.CreateClient();
                    tokenClient.Timeout = TimeSpan.FromSeconds(10);
                    var form = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        ["client_id"]     = clientId,
                        ["client_secret"] = clientSecret,
                        ["refresh_token"] = refreshToken,
                        ["grant_type"]    = "refresh_token"
                    });
                    var tokenResp = await tokenClient.PostAsync("https://oauth2.googleapis.com/token", form);
                    if (tokenResp.IsSuccessStatusCode)
                        return new JsonResult(new { success = true, message = "اتصال Vertex AI برقرار شد — OAuth token دریافت شد." });

                    var errBody = await tokenResp.Content.ReadAsStringAsync();
                    var errNode = System.Text.Json.Nodes.JsonNode.Parse(errBody);
                    var errMsg  = errNode?["error_description"]?.GetValue<string>() ?? errBody[..Math.Min(errBody.Length, 100)];
                    return new JsonResult(new { success = false, message = $"OAuth token refresh ناموفق: {errMsg}" });
                }
                catch (Exception ex)
                {
                    return new JsonResult(new { success = false, message = $"JSON credentials نامعتبر: {ex.Message}" });
                }
            }

            var client = _httpFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            // Google uses X-goog-api-key header instead of Bearer
            bool isGoogle = baseUrl.Contains("generativelanguage.googleapis.com", StringComparison.OrdinalIgnoreCase);
            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/models");
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                if (isGoogle)
                    request.Headers.TryAddWithoutValidation("X-goog-api-key", apiKey);
                else
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            }

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
                return new JsonResult(new { success = true, message = "اتصال با موفقیت برقرار شد." });

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return new JsonResult(new { success = false, message = "API Key نامعتبر است." });

            return new JsonResult(new { success = false, message = $"خطا: {(int)response.StatusCode} {response.ReasonPhrase}" });
        }
        catch (TaskCanceledException)
        {
            return new JsonResult(new { success = false, message = "زمان انتظار به پایان رسید. آدرس را بررسی کنید." });
        }
        catch (HttpRequestException)
        {
            return new JsonResult(new { success = false, message = "اتصال برقرار نشد. آدرس API را بررسی کنید." });
        }
    }

    public async Task<IActionResult> OnPostCheckBalanceAsync([FromBody] CheckBalanceDto dto)
    {
        if (!dto.ProviderId.HasValue)
            return new JsonResult(new { success = false, message = "شناسه سرویس‌دهنده ارسال نشده." });

        var provider = await _providerService.GetProviderByIdAsync(dto.ProviderId.Value);
        if (provider == null)
            return new JsonResult(new { success = false, message = "سرویس‌دهنده یافت نشد." });

        var apiKey = await _providerService.GetDecryptedApiKeyAsync(dto.ProviderId.Value);
        if (string.IsNullOrWhiteSpace(apiKey))
            return new JsonResult(new { success = false, message = "API Key تنظیم نشده است." });

        var baseUrl = provider.BaseUrl.TrimEnd('/');

        try
        {
            var client = _httpFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            // ChatQT: GET /token/usage?token={key}
            if (baseUrl.Contains("chatqt.com", StringComparison.OrdinalIgnoreCase))
            {
                var req = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/token/usage?token={Uri.EscapeDataString(apiKey)}");
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                var resp = await client.SendAsync(req);
                var json = JsonNode.Parse(await resp.Content.ReadAsStringAsync());
                if (resp.IsSuccessStatusCode)
                {
                    var usage = json?["usage"]?.GetValue<double>() ?? 0;
                    return new JsonResult(new { success = true, label = "مصرف کلید", value = $"${usage:F4}", unit = "USD" });
                }
                return new JsonResult(new { success = false, message = "دریافت اطلاعات ناموفق بود." });
            }

            // OpenRouter: GET /auth/key
            if (baseUrl.Contains("openrouter.ai", StringComparison.OrdinalIgnoreCase))
            {
                var req = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/auth/key");
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                var resp = await client.SendAsync(req);
                var json = JsonNode.Parse(await resp.Content.ReadAsStringAsync());
                if (resp.IsSuccessStatusCode)
                {
                    var data = json?["data"];
                    var usage = data?["usage"]?.GetValue<double>() ?? 0;
                    var limit = data?["limit"]?.GetValue<double?>();
                    var remaining = data?["limit_remaining"]?.GetValue<double?>();
                    var valueStr = limit.HasValue
                        ? $"${usage:F4} از ${limit:F2} (باقی‌مانده: ${remaining:F4})"
                        : $"${usage:F4} مصرف شده (بدون سقف)";
                    return new JsonResult(new { success = true, label = "مصرف / موجودی", value = valueStr, unit = "USD" });
                }
                return new JsonResult(new { success = false, message = "دریافت اطلاعات ناموفق بود." });
            }

            // AvalAI: GET /account/balance
            if (baseUrl.Contains("avalai.ir", StringComparison.OrdinalIgnoreCase))
            {
                var req = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/account/balance");
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                var resp = await client.SendAsync(req);
                if (resp.IsSuccessStatusCode)
                {
                    var json = JsonNode.Parse(await resp.Content.ReadAsStringAsync());
                    var balance = json?["balance"]?.GetValue<double>()
                               ?? json?["data"]?["balance"]?.GetValue<double>()
                               ?? 0;
                    return new JsonResult(new { success = true, label = "موجودی", value = $"{balance:N0} تومان", unit = "" });
                }
                return new JsonResult(new { success = false, message = "دریافت اطلاعات ناموفق بود." });
            }

            return new JsonResult(new { success = false, message = "بررسی موجودی برای این سرویس‌دهنده پشتیبانی نمی‌شود." });
        }
        catch (TaskCanceledException)
        {
            return new JsonResult(new { success = false, message = "زمان انتظار به پایان رسید." });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = $"خطا: {ex.Message}" });
        }
    }

    public async Task<IActionResult> OnPostSetCapabilityAsync([FromBody] SetCapabilityDto dto)
    {
        try
        {
            await _providerService.SetCapabilityActiveAsync(dto.ProviderId, dto.Capability, dto.IsActive);
            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    public async Task<IActionResult> OnGetDecryptedKeyAsync(int id)
    {
        var key = await _providerService.GetDecryptedApiKeyAsync(id);
        if (key == null) return new JsonResult(new { success = false });
        return new JsonResult(new { success = true, key });
    }

    public record TestConnectionDto(string BaseUrl, string? ApiKey, int? ProviderId);
    public record CheckBalanceDto(int? ProviderId);
    public record SetCapabilityDto(int ProviderId, string Capability, bool IsActive);
}
