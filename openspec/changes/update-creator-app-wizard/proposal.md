# Change: بهبود Wizard ساخت و ویرایش ابزار

## Why
فرآیند ساخت ابزار مرحله جداگانه‌ای برای فیلدها داشت که باعث می‌شد سازنده پرامپت بنویسد بدون اینکه بداند چه متغیرهایی دارد. همچنین صفحه ویرایش با ساخت متفاوت بود و UX ناهماهنگی ایجاد می‌کرد.

## What Changes
- مرحله ۳ wizard ساخت: فیلدسازی و پرامپت در یک صفحه (دو ستون) ادغام شدند
- Quick-picker: وقتی `{` تایپ می‌شود، فیلدهای تعریف‌شده به صورت dropdown پیشنهاد می‌شوند
- Variable chips: نمایش زنده متغیرهای پرامپت (نارنجی = تعریف‌شده، قرمز = تعریف‌نشده)
- صفحه ویرایش به همان ساختار ۳ مرحله‌ای تبدیل شد
- System Context با راهنمای کلیک‌پذیر (popover) توضیح داده شد
- Redirect پس از ساخت به `/Apps/Index` به جای صفحه Fields

## Impact
- Affected specs: `creator-app-management`
- Affected code:
  - `Areas/Creator/Pages/Apps/Create.cshtml` + `.cs`
  - `Areas/Creator/Pages/Apps/Edit.cshtml` + `.cs`
  - `wwwroot/js/pages/creator/apps/Create.js`
  - `wwwroot/js/pages/creator/apps/Edit.js` (فایل جدید)
  - `Models/AppDto.cs` (اضافه شدن OutputType و AiModelId به UpdateAppDto)
  - `Services/AppService.cs`

## Status
پیاده‌سازی کامل شده.
