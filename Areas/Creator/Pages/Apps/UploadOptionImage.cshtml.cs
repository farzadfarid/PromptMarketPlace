using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Areas.Creator.Pages.Apps;

[Authorize(Policy = "CreatorOnly")]
public class UploadOptionImageModel : PageModel
{
    private readonly IStorageService _storage;

    public UploadOptionImageModel(IStorageService storage) => _storage = storage;

    public async Task<IActionResult> OnPostAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return new JsonResult(new { error = "فایل انتخاب نشده." }) { StatusCode = 400 };

        if (file.Length > 5 * 1024 * 1024)
            return new JsonResult(new { error = "حداکثر حجم فایل ۵ مگابایت است." }) { StatusCode = 400 };

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext is not (".jpg" or ".jpeg" or ".png" or ".webp"))
            return new JsonResult(new { error = "فرمت تصویر پشتیبانی نمی‌شود." }) { StatusCode = 400 };

        var url = await _storage.SaveUploadAsync(file, "styles");
        return new JsonResult(new { url });
    }
}
