using System.Net.Http.Headers;
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
            await _providerService.CreateProviderAsync(Form.Name, Form.BaseUrl, Form.ApiKey, Form.Description);
        else
            await _providerService.UpdateProviderAsync(Form.Id, Form.Name, Form.BaseUrl, Form.ApiKey, Form.Description);

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
            var client = _httpFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/models");
            if (!string.IsNullOrWhiteSpace(apiKey))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

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

    public record TestConnectionDto(string BaseUrl, string? ApiKey, int? ProviderId);
}
