# Change: افزودن Strategy Pattern برای پشتیبانی از چند سرویس‌دهنده AI

## Why

پروژه از چندین سرویس‌دهنده مختلف استفاده می‌کند (AvalAI، OpenAI، Anthropic، ChatQT) که هر کدام فرمت API متفاوت دارند. کد قبلی در `AiService.cs` تمام این تفاوت‌ها را با fallback chain حدسی مدیریت می‌کرد — برای مثال ابتدا `video/generations` می‌زد و اگر 4xx برگشت، به `videos/` می‌رفت. این رویکرد باعث می‌شد هر request غیرضروری چند endpoint بزند و debug آن سخت باشد. همچنین اضافه کردن سرویس‌دهنده جدید مستلزم دست زدن به منطق اصلی بود.

## What Changes

- **BREAKING**: فیلد `ProviderType` به موجودیت `AiProvider` اضافه شد (Migration لازم است)
- افزودن enum `ProviderType`: `OpenAiCompatible=0`, `AvalAi=1`, `Anthropic=2`, `ChatQt=3`
- پیاده‌سازی Strategy Pattern با `IProviderStrategy` interface و 4 استراتژی مجزا
- `AiService` به یک dispatcher ساده تبدیل شد که بر اساس `ProviderType` استراتژی را انتخاب می‌کند
- افزودن متد `IStorageService.SaveBytesAsync` برای ذخیره داده‌های باینری (صدا از AvalAI)
- پشتیبانی واقعی از Anthropic: `/messages` endpoint با `x-api-key` header
- پشتیبانی واقعی از ChatQT: تصویر/ویدیو از `chat/completions` با پارسینگ `images`/`videos` array
- ادمین در پنل سرویس‌دهنده‌ها می‌تواند نوع هر provider را تعیین کند
- `ExecutionService` برای صدای ذخیره‌شده local (شروع با `/uploads/`) دانلود نمی‌کند

## Impact

- Affected specs: `ai-providers`, `services-interfaces`
- Affected code:
  - `Services/AiService.cs` (بازنویسی کامل → dispatcher)
  - `Services/Strategies/` (8 فایل جدید)
  - `Models/Domain/AiProvider.cs` (فیلد ProviderType)
  - `Models/Enums/ProviderType.cs` (enum جدید)
  - `Services/Interfaces/IStorageService.cs` (متد جدید)
  - `Services/LocalStorageService.cs` (پیاده‌سازی متد جدید)
  - `Services/Interfaces/IAiProviderService.cs` (signature تغییر کرد)
  - `Services/AiProviderService.cs` (ProviderType در Create/Update)
  - `ViewModels/Admin/AiProviderViewModel.cs` (فیلد ProviderType)
  - `Areas/Admin/Pages/AI/Providers.cshtml` (dropdown نوع سرویس‌دهنده)
  - `Areas/Admin/Pages/AI/Providers.cshtml.cs` (پاس دادن ProviderType)
  - `Program.cs` (ثبت 5 سرویس جدید)
  - `Services/ExecutionService.cs` (مدیریت مسیر local برای صدا)
  - `wwwroot/js/pages/admin/Providers.js` (بروزرسانی openEdit)
- Migration: `AddProviderType` اعمال شده

## نکته مهم برای ادمین

پس از اعمال migration، ProviderType همه providerهای موجود به `0 = OpenAiCompatible` تنظیم می‌شود. ادمین باید provider AvalAI را در پنل ویرایش کرده و نوع را به **AvalAI** تغییر دهد تا ویدیو و صدا درست کار کنند.
