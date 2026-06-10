## ADDED Requirements

### Requirement: Actual Token Usage Stats — Admin App Detail
صفحه `Admin/Apps/Detail` SHALL کارت "مصرف توکن واقعی" را در ستون چپ، بین کارت "هزینه اجرا" و "فیلدهای ورودی" نمایش دهد.

منبع داده: `AppExecution.TokensUsed` (nullable int) در رکوردهایی با `Status == Completed`.

PageModel properties:
- `int? AvgTokensAll` — میانگین کل اجراهای موفق
- `int? AvgTokens30d` — میانگین ۳۰ روز اخیر
- `bool IsUnprofitable` — true اگر `AvgTokens30d > CreditCost * TokensPerCredit`
- `const int TokensPerCredit = 1000`

#### Scenario: هزینه اعتبار همیشه نمایان
- **WHEN** صفحه Detail ابزار بارگذاری می‌شود، صرف نظر از وجود داده TokensUsed
- **THEN** کارت "مصرف توکن واقعی" رندر می‌شود
- **THEN** ردیف "هزینه اعتبار تنظیم شده" با مقدار `App.CreditCost` نمایش داده می‌شود
- **THEN** ردیف "ظرفیت هر اعتبار: ~۱٬۰۰۰ توکن" نمایش داده می‌شود

#### Scenario: نمایش میانگین توکن‌ها وقتی داده موجود است
- **WHEN** حداقل یک اجرای موفق با `TokensUsed != null` وجود دارد
- **THEN** ردیف "میانگین توکن (کل)" با مقدار `AvgTokensAll.N0Fa()` نمایش داده می‌شود
- **WHEN** حداقل یک اجرای موفق در ۳۰ روز اخیر با `TokensUsed != null` وجود دارد
- **THEN** ردیف "میانگین توکن (۳۰ روز)" نمایش داده می‌شود

#### Scenario: هشدار زیان‌دهی
- **WHEN** `IsUnprofitable == true` (AvgTokens30d > CreditCost * 1000)
- **THEN** badge قرمز "احتمال زیان‌دهی" در header کارت نمایش داده می‌شود
- **THEN** مقدار AvgTokens30d با کلاس `text-danger` رندر می‌شود
- **THEN** `alert-danger` با پیام "افزایش هزینه اجرا یا بهینه‌سازی پرامپت توصیه می‌شود" نمایش می‌یابد

#### Scenario: بدون داده توکن
- **WHEN** هیچ اجرای موفقی با TokensUsed ثبت‌شده وجود ندارد
- **THEN** پیام "هنوز اجرایی با ثبت توکن وجود ندارد" زیر dl نمایش داده می‌شود

---

### Requirement: Actual Token Usage Stats — Creator App Edit
صفحه `Creator/Apps/Edit` SHALL نوار اطلاعاتی مصرف توکن را قبل از wizard steps نمایش دهد (فقط اگر داده موجود باشد).

#### Scenario: نوار آمار در حالت عادی
- **WHEN** `AvgTokensAll != null` و `IsUnprofitable == false`
- **THEN** `alert-info` با میانگین توکن کل، میانگین ۳۰ روز (در صورت وجود)، هزینه اجرا نمایش داده می‌شود
- **THEN** آیکون `?` برای راهنما در کنار alert قرار دارد

#### Scenario: نوار آمار با هشدار زیان‌دهی
- **WHEN** `IsUnprofitable == true`
- **THEN** `alert-danger` با badge "احتمال زیان‌دهی — افزایش هزینه اجرا توصیه می‌شود" نمایش داده می‌شود

#### Scenario: بدون داده توکن
- **WHEN** `AvgTokensAll == null`
- **THEN** نوار آمار اصلاً رندر نمی‌شود

---

### Requirement: Token Analytics Data Query
هر دو PageModel SHALL داده توکن را به روش زیر query کنند:

```
var tokenData = await _db.Executions
    .Where(e => e.AppId == id
             && e.Status == ExecutionStatus.Completed
             && e.TokensUsed.HasValue)
    .Select(e => new { e.TokensUsed, e.CreatedAt })
    .ToListAsync();
```

#### Scenario: محاسبه میانگین‌ها
- **WHEN** `tokenData.Any() == true`
- **THEN** `AvgTokensAll = (int)tokenData.Average(e => (double)e.TokensUsed.Value)`
- **THEN** رکوردهای ۳۰ روز اخیر: `tokenData.Where(e => e.CreatedAt >= DateTime.UtcNow.AddDays(-30))`
- **THEN** اگر داده ۳۰ روزه وجود داشته باشد: `AvgTokens30d` محاسبه و `IsUnprofitable` ارزیابی می‌شود

---

### Requirement: Token Estimation Calculator
هر دو صفحه `Admin/Apps/Detail` و `Creator/Apps/Edit` SHALL کارت Collapsible "ماشین حساب تخمین توکن" داشته باشند که تخمین مصرف توکن برای هر اجرا را نمایش دهد.

Server-side properties:
- **Admin**: `int PromptCharCount => DecryptedPrompt.Length` (computed)
- **Admin**: `int SystemContextCharCount => App?.SystemContext?.Length ?? 0` (computed)
- **Creator**: `int PromptCharCount` — از `_encryption.Decrypt(app.EncryptedPrompt).Length` (یک بار decrypt)
- **Creator**: `int SystemContextCharCount = app.SystemContext?.Length ?? 0`

Client-side data attributes روی container div:
- `data-prompt="@Model.PromptCharCount"`
- `data-sys="@Model.SystemContextCharCount"`
- `data-fields="@(JsonSerializer.Serialize(fields.Select(f => f.Type.ToString())))"`

#### Scenario: محاسبه اولیه هنگام لود
- **WHEN** JavaScript اجرا می‌شود
- **THEN** `sysTokens = Math.round(sysChars / 3.5)`
- **THEN** `promptTokens = Math.round(promptChars / 3.5)`
- **THEN** `fieldTokens = fields.reduce((s,t) => s + FIELD_TOKENS[t], 0)` که `FIELD_TOKENS = {Textarea:300, Text:50, Select:5, Number:5, Checkbox:3}`
- **THEN** مجموع با خروجی پیش‌فرض ۵۰۰ محاسبه و نمایش داده می‌شود

#### Scenario: تغییر slider خروجی
- **WHEN** کاربر slider "خروجی تخمینی" را تغییر می‌دهد (range 100-4000، step 100)
- **THEN** `total = sysTokens + promptTokens + fieldTokens + outputVal`
- **THEN** `credits = Math.ceil(total / 1000)` محاسبه و بلافاصله نمایش داده می‌شود
- **THEN** اعداد با `toLocaleString('fa-IR')` فارسی نمایش داده می‌شوند

#### Scenario: Collapsible toggle
- **WHEN** کاربر دکمه "نمایش / پنهان" را کلیک می‌کند
- **THEN** Bootstrap Collapse محتوای calculator را نمایش می‌دهد یا پنهان می‌کند

---

### Requirement: Contextual Help Popovers for Token Sections
هر دو کارت "مصرف توکن واقعی" و "ماشین حساب تخمین توکن" در هر دو صفحه ادمین و سازنده SHALL آیکون راهنما `?` داشته باشند.

محتوای راهنما از طریق JS object تعریف می‌شود (نه data attribute) تا از مشکل HTML escaping در attribute جلوگیری شود.

#### Scenario: باز شدن popover با کلیک
- **WHEN** کاربر روی آیکون `?` کلیک می‌کند
- **THEN** `bootstrap.Popover` با `html:true`, `trigger:'click'`, `customClass:'help-popover'` نمایش داده می‌شود
- **THEN** محتوا شامل توضیح فارسی هر metric با فرمت `<dl>` است

#### Scenario: محتوای راهنمای "مصرف توکن واقعی"
- **WHEN** popover کارت "مصرف توکن واقعی" باز می‌شود
- **THEN** توضیح می‌دهد: میانگین کل، میانگین ۳۰ روز، ظرفیت هر اعتبار (۱٬۰۰۰ توکن)، و شرط هشدار زیان‌دهی `avgTokens30d > creditCost × 1000`

#### Scenario: محتوای راهنمای "ماشین حساب"
- **WHEN** popover کارت "ماشین حساب" باز می‌شود
- **THEN** توضیح می‌دهد: فرمول chars÷3.5، وزن هر نوع فیلد، راهنمای تنظیم slider، و فرمول `⌈total÷1000⌉` برای هزینه پیشنهادی

#### Scenario: بسته شدن popover
- **WHEN** کاربر روی هر جایی خارج از `.help-popover-btn` یا `.popover` کلیک می‌کند
- **THEN** `bootstrap.Popover.getInstance(btn).hide()` برای همه popover های باز فراخوانی می‌شود

#### Scenario: ترتیب لود script در ادمین
- **WHEN** صفحه `Admin/Apps/Detail` رندر می‌شود
- **THEN** script مقداردهی popover در `@section Scripts` قرار دارد
- **THEN** این section پس از `<script src="~/js/bootstrap5-3-3.js">` در layout اجرا می‌شود
- **THEN** `bootstrap.Popover` در زمان اجرا در دسترس است

---

### Requirement: Help Popover Styles
فایل `wwwroot/css/site.css` SHALL استایل‌های لازم برای `.help-popover` و `.help-popover-btn` را تعریف کند.

#### Scenario: RTL و اندازه popover
- **WHEN** popover با کلاس `help-popover` نمایش داده می‌شود
- **THEN** `direction:rtl`, `max-width:320px` اعمال می‌شود
- **THEN** `popover-header` با `font-weight:600`, `font-size:.85rem` نمایش داده می‌شود
- **THEN** `popover-body` با `font-size:.82rem`, `line-height:1.7` رندر می‌شود

#### Scenario: استایل آیکون راهنما
- **WHEN** `.help-popover-btn` رندر می‌شود
- **THEN** `opacity:0.6` در حالت عادی و `opacity:1` در hover اعمال می‌شود
