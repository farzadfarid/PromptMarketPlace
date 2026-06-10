# Change: File Upload & Vision Support for AI Tools

## Why

ابزارهای ویدیوسازی و ویرایش تصویر به ورودی تصویر از کاربر نیاز دارند. فیلد `FileUpload` در enum وجود داشت اما نه در فرم عمومی رندر می‌شد و نه در پایپلاین اجرا پشتیبانی داشت.

## What Changes

- فیلد `FileUpload` در فرم عمومی Detail به صورت drag-and-drop با پیش‌نمایش رندر می‌شود
- فرم ارسال ابزار به `multipart/form-data` تغییر یافت تا آپلود فایل را پشتیبانی کند
- `IStorageService` متد `SaveUploadAsync(IFormFile, folder)` دریافت کرد
- `IAiService.RunAsync` پارامتر `List<string>? inputImageUrls` دریافت کرد
- `RunChatCompletionAsync` هنگام وجود تصویر، پیام‌های multimodal (vision) ارسال می‌کند
- `RunVideoGenerationAsync` آرایه `images` را در request body به API ارسال می‌کند
- `IExecutionService.ExecuteAsync` پارامتر `List<string>? inputImageUrls` دریافت کرد
- سازنده می‌تواند در Fields page فیلد از نوع `FileUpload` تعریف کند

## Impact

- Affected specs: `06-public-pages`, `07-creator-area`, `05-services-interfaces`
- Affected code:
  - `Services/Interfaces/IStorageService.cs` — SaveUploadAsync
  - `Services/LocalStorageService.cs` — پیاده‌سازی SaveUploadAsync
  - `Services/Interfaces/IAiService.cs` — inputImageUrls param
  - `Services/AiService.cs` — vision multimodal + video images
  - `Services/Interfaces/IExecutionService.cs` — inputImageUrls param
  - `Services/ExecutionService.cs` — پاس دادن image URLs به AI
  - `Helpers/InputValidator.cs` — رد کردن MaxLength/MinLength برای FileUpload
  - `Pages/App/Detail.cshtml[.cs]` — multipart form + FileUpload rendering
  - `Areas/Creator/Pages/Apps/Fields.cshtml` — آپشن FileUpload
