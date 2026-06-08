# Change: UI رنگ‌بندی نارنجی، کارت ابزارها، آپلود تصویر، UX اجرا، قیمت‌گذاری اعتبار، محلی‌سازی فارسی

## Why
رنگ اصلی سایت از آبی/سبز Bootstrap به نارنجی برند تغییر کرد. همزمان نیاز به بهبود کارت ابزارها، آپلود تصویر کاور، تجربه اجرای ابزار، سیستم قیمت‌گذاری اعتبار، و محلی‌سازی کامل اعداد/تاریخ به فارسی وجود داشت.

## What Changes

### ۱. سیستم رنگ (frontend-theme)
- Override کامل Bootstrap CSS variables: `--bs-primary`, `--bs-success`, `--bs-info` به نارنجی
- رفع تمام رنگ‌های هاردکد آبی/سبز در JS، CSS و CSHTML
- گرادیان hero صفحه اصلی به `--pm-gradient-hero`
- `alert-success`, `alert-info`, `btn-success`, `btn-info`, `card-gradient-*` همه نارنجی
- `--bs-table-color` در هر سه dark layout برای دید صحیح متن جداول

### ۲. کارت ابزار (app-card)
- طراحی کامل جدید کلاس `.app-card` با thumbnail، badge نوع خروجی، hover effect
- رفع باگ Razor switch expression که raw text رندر می‌شد
- گرادیان placeholder متفاوت برای هر OutputType
- نمایش thumbnail در لیست ابزارهای creator

### ۳. آپلود تصویر کاور (thumbnail-upload)
- فیلد `IFormFile? Thumbnail` در Create/Edit forms
- ذخیره فایل در `wwwroot/uploads/thumbnails/` با نام UUID
- `ThumbnailUrl` به `CreateAppDto` و `UpdateAppDto` اضافه شد
- upload box با aspect-ratio 750/404، preview آنی، راهنمای اندازه
- ویرایش thumbnail مستقل از وضعیت ابزار (فقط prompt نیاز به Draft دارد)

### ۴. UX اجرا (execution-ux)
- حفظ مقادیر فرم بعد از خطای اجرا (`Model.Inputs` پر می‌شود)
- نمایش خطای اجرای ناموفق در `_OutputRenderer`
- dashboard فقط اجراهای موفق نشون می‌دهد
- تاریخچه اجراها: همه ردیف‌ها کلیک‌پذیر
- اجرای مجدد با pre-fill ورودی‌های قبلی از exec ناموفق
- دکمه خروج از حساب در Creator و Admin layouts
- bypass پرداخت برای تست (شارژ مستقیم بدون درگاه)
- پیام خطای دقیق از OpenRouter (extract از JSON)
- پارامتر `max_tokens` به request AI اضافه شد

### ۵. قیمت‌گذاری اعتبار (credit-pricing)
- صفحه جدید ادمین `/Admin/Credits/Pricing`
- فرمول: هزینه دلاری × نرخ دلار × (1+VAT) × (1+Margin) ÷ ارزش اعتبار
- تنظیمات: نرخ دلار، VAT 9%، سود پلتفرم، ارزش اعتبار، میانگین توکن
- جدول محاسبه خودکار برای هر مدل فعال
- تعرفه نهایی جداگانه برای متن/تصویر/ویدیو/صدا
- لینک منابع: tgju.org، bonbast.com، openrouter.ai/models
- دکمه "اعمال به همه ابزارها" در صفحه مدل‌های AI

### ۶. محلی‌سازی فارسی (persian-locale)
- `PersianHelper.cs`: متدهای `ToShamsi()`, `N0Fa()`, `ToFarsiDigits()`
- اعمال در ۲۴ فایل CSHTML
- `using PromptMarketPlace.Helpers` در همه `_ViewImports.cshtml`

## Impact
- Affected specs: frontend-conventions, creator-area, admin-area, user-area, execution-engine, payment
- Affected code: site.css، تمام layout CSS، _AppCard.cshtml، Create/Edit/Detail، AiService، ExecutionService، AppService، AppDto، PersianHelper (جدید)
- SQL لازم: seed_pricing_settings.sql، fix_encrypted_prompts.sql، fix_dropdown_persian.sql
