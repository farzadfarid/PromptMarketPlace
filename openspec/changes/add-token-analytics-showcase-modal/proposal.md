# Change: آنالیتیکس توکن واقعی، ماشین حساب تخمین، راهنمای تعاملی، و مودال نمونه خروجی

## Why
سازندگان و ادمین‌ها نیاز داشتند بدانند ابزارشان واقعاً چه مقدار توکن مصرف می‌کند و آیا هزینه اعتباری که تنظیم کرده‌اند برای پوشش هزینه API کافی است یا خیر. پیش از این داده TokensUsed در مدل موجود بود ولی هیچ جایی نمایش داده نمی‌شد و امکان مقایسه با هزینه اعتبار وجود نداشت. همچنین نمونه خروجی‌های صفحه عمومی ابزار تنها ۲ خط نمایش می‌دادند و کاربر محتوای کامل را نمی‌دید.

## What Changes

### ۱. مودال نمونه خروجی (showcase-modal)
- هر showcase item در صفحه عمومی `/app/{slug}` کلیک‌پذیر شد
- کلیک روی آیتم، Bootstrap Modal باز می‌کند و محتوای کامل را نمایش می‌دهد
- برای خروجی متنی: `<pre>` با `white-space:pre-wrap` و فونت طبیعی
- برای خروجی تصویری: `<img class="img-fluid rounded">`
- Caption آیتم به عنوان title مودال استفاده می‌شود
- پیش‌نمایش کارت: حداکثر ۵۶ پیکسل ارتفاع + لینک "مشاهده کامل"
- داده از طریق `data-caption`, `data-text`, `data-imgurl`, `data-type` به JS منتقل می‌شود (Razor auto-encode)

### ۲. آمار مصرف توکن واقعی (token-analytics)
- در صفحه **Admin/Apps/Detail**: کارت جدید "مصرف توکن واقعی" در ستون چپ
  - هزینه اعتبار تنظیم‌شده (همیشه نمایان، حتی بدون داده)
  - ظرفیت هر اعتبار: ~۱٬۰۰۰ توکن
  - میانگین توکن کل (اگر داده موجود باشد)
  - میانگین توکن ۳۰ روز اخیر (اگر داده موجود باشد)
  - هشدار `alert-danger` اگر `AvgTokens30d > CreditCost × 1000`
- در صفحه **Creator/Apps/Edit**: نوار اطلاعاتی قبل از wizard steps
  - `alert-info` در حالت عادی
  - `alert-danger` + badge "احتمال زیان‌دهی" در حالت زیان‌دهی
- منبع داده: `AppExecution.TokensUsed` (فیلد nullable int، از قبل موجود)
- ثابت: `TokensPerCredit = 1000` در هر دو PageModel

### ۳. ماشین حساب تخمین توکن (token-calculator)
- در هر دو صفحه ادمین و سازنده، کارت Collapsible "ماشین حساب تخمین توکن"
- ورودی‌های ثابت از server-side:
  - `PromptCharCount`: طول پرامپت رمزگشایی‌شده (chars)
  - `SystemContextCharCount`: طول System Context (chars)
  - لیست نوع فیلدها به صورت JSON در `data-fields`
- تبدیل به توکن: `chars ÷ 3.5`
- وزن فیلدهای ورودی: `Textarea=300, Text=50, Select=5, Number=5, Checkbox=3`
- ورودی تعاملی: slider "خروجی تخمینی" از ۱۰۰ تا ۴۰۰۰ (پیش‌فرض ۵۰۰)
- خروجی: مجموع توکن + هزینه پیشنهادی `⌈total ÷ 1000⌉` اعتبار
- JS در `@section Scripts` برای هر دو صفحه

### ۴. راهنمای تعاملی (help-popovers)
- آیکون `?` (fas fa-question-circle) در header هر دو کارت
- Bootstrap Popover با `html: true`, `trigger: 'click'`, `placement: 'auto'`
- محتوای کامل فارسی برای هر بخش:
  - **مصرف توکن واقعی**: توضیح هر metric (میانگین کل، میانگین ۳۰ روز، ظرفیت اعتبار، شرط هشدار)
  - **ماشین حساب**: توضیح فرمول محاسبه هر سطر (System Context، پرامپت، فیلدها، خروجی، هزینه پیشنهادی)
- بسته شدن با کلیک بیرون از popover (document click handler)
- CSS کلاس `.help-popover` در site.css برای RTL، اندازه، و فاصله‌گذاری
- **نکته مهم**: برای ادمین، script در `@section Scripts` قرار گرفت تا پس از لود `bootstrap5-3-3.js` اجرا شود

## Impact

- **Affected specs**: showcase-modal, token-analytics
- **Affected code**:
  - `Pages/App/Detail.cshtml` — showcase items + modal + JS handler
  - `wwwroot/js/pages/public/Detail.js` — showcase click → modal populate
  - `Areas/Admin/Pages/Apps/Detail.cshtml` — کارت‌های token stats + calculator + popovers
  - `Areas/Admin/Pages/Apps/Detail.cshtml.cs` — AvgTokensAll, AvgTokens30d, IsUnprofitable, PromptCharCount, SystemContextCharCount
  - `Areas/Creator/Pages/Apps/Edit.cshtml` — نوار token stats + کارت calculator + popovers
  - `Areas/Creator/Pages/Apps/Edit.cshtml.cs` — همان properties + رفع double-decrypt
  - `wwwroot/css/site.css` — `.help-popover`, `.help-popover-btn` styles
- **No migrations needed**: TokensUsed از قبل در AppExecution موجود بود
