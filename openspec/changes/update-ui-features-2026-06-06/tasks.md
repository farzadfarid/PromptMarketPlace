## 1. سیستم رنگ (frontend-theme)
- [x] 1.1 Override `--bs-primary`, `--bs-success`, `--bs-info` در site.css
- [x] 1.2 رفع هاردکد آبی/سبز در JS (Dashboard.js admin و creator)
- [x] 1.3 رفع آبی در ForgotPassword email template
- [x] 1.4 رفع آبی در Fields.cshtml preview
- [x] 1.5 گرادیان hero صفحه اصلی
- [x] 1.6 Alert، btn-success، card-gradient همه نارنجی
- [x] 1.7 `--bs-table-color` در admin/creator/user layout CSS
- [x] 1.8 `form-text` رنگ مناسب در dark layouts

## 2. کارت ابزار (app-card)
- [x] 2.1 بازنویسی `_AppCard.cshtml` با ساختار جدید
- [x] 2.2 CSS کلاس `.app-card` با hover، badge، thumbnail
- [x] 2.3 رفع Razor switch expression bug
- [x] 2.4 Wizard tab: رنگ متن مشکی، toggle `text-muted`
- [x] 2.5 نمایش thumbnail در لیست ابزارهای creator (48×48)

## 3. آپلود تصویر کاور (thumbnail-upload)
- [x] 3.1 `ThumbnailUrl` به `CreateAppDto` و `UpdateAppDto`
- [x] 3.2 `AppService.CreateAppAsync` و `UpdateAppAsync` آپدیت
- [x] 3.3 `IFormFile? Thumbnail` و `SaveThumbnailAsync` در Create/Edit page models
- [x] 3.4 `enctype="multipart/form-data"` در فرم‌ها
- [x] 3.5 Upload box با aspect-ratio 750/404، preview JS، max-width
- [x] 3.6 رفع محدودیت ویرایش ابزار فعال (فقط prompt محدود)

## 4. UX اجرا (execution-ux)
- [x] 4.1 حفظ ورودی‌های فرم بعد از خطا (`Model.Inputs`)
- [x] 4.2 `_OutputRenderer` نمایش خطا برای Status=Failed
- [x] 4.3 Dashboard فقط Completed executions
- [x] 4.4 تاریخچه اجراها: همه ردیف‌ها کلیک‌پذیر
- [x] 4.5 اجرای مجدد با exec param و pre-fill ورودی‌ها
- [x] 4.6 دکمه خروج در Creator و Admin layouts
- [x] 4.7 Bypass پرداخت برای تست
- [x] 4.8 Error message دقیق از OpenRouter
- [x] 4.9 `max_tokens` در AI request
- [x] 4.10 Validation summary empty div مخفی (JS)

## 5. قیمت‌گذاری اعتبار (credit-pricing)
- [x] 5.1 SystemSettings جدید برای Pricing group (SQL)
- [x] 5.2 `Pricing.cshtml` و `Pricing.cshtml.cs` در Admin/Credits
- [x] 5.3 فرمول کامل با لینک منابع
- [x] 5.4 جدول محاسبه خودکار per model
- [x] 5.5 منوی ادمین "تعرفه اعتبار"
- [x] 5.6 دکمه "اعمال به همه ابزارها" در صفحه مدل‌ها

## 6. محلی‌سازی فارسی (persian-locale)
- [x] 6.1 `Helpers/PersianHelper.cs` ساخته شد
- [x] 6.2 `@using PromptMarketPlace.Helpers` در همه _ViewImports
- [x] 6.3 Replace `ToString("yy/MM/dd")` → `.ToShamsi()` در ۲۴ فایل
- [x] 6.4 Replace `ToString("N0")` → `.N0Fa()` در ۲۴ فایل
- [x] 6.5 گزینه‌های dropdown فارسی (SQL)

## 7. رفع خطاها و بیلد
- [x] 7.1 رفع `CS0103 Inputs` → `Model.Inputs`
- [x] 7.2 رفع `RZ1031` option selected attribute
- [x] 7.3 رفع duplicate using در _ViewImports
- [x] 7.4 رفع `CS0266` decimal/double cast در Pricing
- [x] 7.5 بیلد نهایی موفق بدون error
