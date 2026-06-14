## ADDED Requirements

### Requirement: ذخیره و مدیریت اعلان‌ها
سیستم SHALL اعلان‌ها را به ازای هر کاربر در دیتابیس ذخیره کند و امکان خواندن، شمارش، و علامت‌گذاری به عنوان خوانده‌شده را فراهم کند.

Entity `Notification` دارای فیلدهای: Id، UserId، Title، Message?، Link?، Category (app_review|app_status|open_prompt|review|withdrawal|general)، IsRead، CreatedAt.

#### Scenario: ایجاد اعلان تکی
- **WHEN** `INotificationService.CreateAsync(userId, title, message, link, category)` فراخوانی شود
- **THEN** یک رکورد `Notification` با IsRead=false در دیتابیس ثبت شود

#### Scenario: ایجاد اعلان برای همه ادمین‌ها
- **WHEN** `CreateForAdminsAsync(title, message, link, category)` فراخوانی شود
- **THEN** یک رکورد Notification برای هر کاربر با نقش Admin ایجاد شود

#### Scenario: دریافت تعداد خوانده‌نشده
- **WHEN** `GetUnreadCountAsync(userId)` فراخوانی شود
- **THEN** تعداد رکوردهای IsRead=false برای آن کاربر برگردانده شود

#### Scenario: علامت‌گذاری همه به عنوان خوانده‌شده
- **WHEN** `MarkAllReadAsync(userId)` فراخوانی شود
- **THEN** تمام اعلان‌های IsRead=false آن کاربر به IsRead=true تغییر کنند

### Requirement: API اعلان‌ها
سیستم SHALL یک API Razor Page در مسیر `/api/notifications` برای polling و mark-all-read فراهم کند.

#### Scenario: دریافت تعداد
- **WHEN** کلاینت `GET /api/notifications?handler=unread` را فراخوانی کند
- **THEN** `{"count": N}` با وضعیت 200 برگردانده شود (یا 401 اگر احراز هویت نشده)

#### Scenario: دریافت ۱۰ اعلان اخیر
- **WHEN** کلاینت `GET /api/notifications?handler=recent` را فراخوانی کند
- **THEN** آرایه JSON با حداکثر ۱۰ آیتم شامل id، title، message، link، category، isRead، createdAt برگردانده شود

#### Scenario: خواندن همه از طریق API
- **WHEN** کلاینت `POST /api/notifications?handler=markAllRead` را فراخوانی کند
- **THEN** همه اعلان‌های کاربر جاری به خوانده‌شده تبدیل شوند و `{"ok": true}` برگردانده شود

### Requirement: نمایش زنگ اعلان در topbar
هر دو پنل ادمین و سازنده SHALL یک آیکون زنگ با badge تعداد خوانده‌نشده در topbar داشته باشند.

داده اولیه از طریق SSR (Razor) در layout خوانده می‌شود تا اولین render بدون تاخیر باشد.

#### Scenario: نمایش badge
- **WHEN** کاربر صفحه‌ای در پنل را باز کند و اعلان خوانده‌نشده داشته باشد
- **THEN** badge بنفش/نارنجی با تعداد روی آیکون زنگ نمایش داده شود

#### Scenario: badge نمایش داده نمی‌شود
- **WHEN** تعداد خوانده‌نشده صفر باشد
- **THEN** badge مخفی باشد

#### Scenario: باز شدن dropdown
- **WHEN** کاربر روی آیکون زنگ کلیک کند
- **THEN** dropdown گلاس‌مورفیسم با ۱۰ اعلان اخیر نمایش داده شود؛ هر آیتم دارای آیکون رنگی بر اساس category، عنوان، پیام کوتاه، و زمان نسبی فارسی باشد

#### Scenario: dropdown در RTL برش نخورد
- **WHEN** dropdown در layout RTL فارسی باز شود
- **THEN** dropdown به سمت راست باز شود (left: 0) و از لبه صفحه خارج نشود

#### Scenario: خواندن همه
- **WHEN** کاربر دکمه «همه خوانده شد» را در dropdown کلیک کند
- **THEN** `POST /api/notifications?handler=markAllRead` ارسال شود و badge به صفر برسد

### Requirement: polling خودکار
مرورگر SHALL هر ۳۰ ثانیه تعداد اعلان‌های خوانده‌نشده را poll کند و badge را بروزرسانی کند.

#### Scenario: بروزرسانی badge
- **WHEN** ۳۰ ثانیه از آخرین poll گذشته باشد
- **THEN** `GET /api/notifications?handler=unread` فراخوانی شود و badge بدون reload صفحه بروزرسانی شود

#### Scenario: poll اول baseline
- **WHEN** صفحه بارگذاری شود و اولین poll انجام شود
- **THEN** `lastSeenMaxId` از آخرین آیتم موجود مقداردهی شود بدون اینکه toast نمایش داده شود

### Requirement: toast notification برای اعلان‌های جدید
سیستم SHALL وقتی اعلان جدید (بعد از بارگذاری صفحه) دریافت می‌شود، یک toast card در گوشه چپ‌بالا نمایش دهد.

#### Scenario: نمایش toast
- **WHEN** poll بعدی نشان دهد تعداد اعلان بیشتر شده
- **THEN** برای هر اعلان جدید (id > lastSeenMaxId) یک toast با عنوان، پیام، آیکون category، و نوار رنگی accent نمایش داده شود

#### Scenario: auto-dismiss
- **WHEN** toast نمایش داده شود
- **THEN** progress bar در پایین toast در ۶ ثانیه خالی شود و toast به صورت smooth خارج شود

#### Scenario: hover pause
- **WHEN** کاربر ماوس را روی toast نگه دارد
- **THEN** تایمر auto-dismiss متوقف شود؛ با خروج ماوس، با ۱.۵ ثانیه باقی‌مانده از سر گرفته شود

#### Scenario: stack
- **WHEN** چند اعلان همزمان رسیده باشند
- **THEN** حداکثر ۳ toast همزمان نمایش داده شوند (stack از بالا به پایین)

### Requirement: تریگرهای Creator → Admin
سیستم SHALL هنگام اقدامات سازنده، اعلان مناسب برای ادمین‌ها ارسال کند.

#### Scenario: ارسال ابزار برای بررسی
- **WHEN** سازنده ابزار را از Draft به UnderReview تغییر دهد
- **THEN** اعلان با category `app_review` و لینک به صفحه Admin/Apps/{id} برای همه ادمین‌ها ارسال شود

#### Scenario: درخواست پرامپت باز
- **WHEN** سازنده IsPromptPublicRequested=true کند
- **THEN** اعلان با category `open_prompt` برای همه ادمین‌ها ارسال شود

#### Scenario: لغو درخواست پرامپت باز
- **WHEN** سازنده IsPromptPublicRequested=false کند
- **THEN** اعلان «لغو درخواست» با category `open_prompt` برای همه ادمین‌ها ارسال شود

#### Scenario: درخواست برداشت
- **WHEN** سازنده درخواست برداشت ثبت کند
- **THEN** اعلان با category `withdrawal` برای همه ادمین‌ها ارسال شود

#### Scenario: پیام جدید از سازنده
- **WHEN** سازنده thread جدید ایجاد کند یا در thread موجود پاسخ دهد
- **THEN** اعلان با category `general` برای همه ادمین‌ها ارسال شود

### Requirement: تریگرهای Admin → Creator
سیستم SHALL هنگام اقدامات ادمین روی ابزار یا برداشت، اعلان برای سازنده ارسال کند.

#### Scenario: تایید ابزار
- **WHEN** ادمین ابزار را از Review تایید کند
- **THEN** اعلان «ابزار تایید شد» با category `app_review` برای creator ارسال شود

#### Scenario: رد ابزار
- **WHEN** ادمین ابزار را رد کند
- **THEN** اعلان «ابزار رد شد» با دلیل رد و category `app_review` برای creator ارسال شود

#### Scenario: تغییر وضعیت ابزار
- **WHEN** ادمین وضعیت ابزار را به Active یا Suspended تغییر دهد
- **THEN** اعلان با category `app_status` برای creator ارسال شود

#### Scenario: تایید/لغو پرامپت باز
- **WHEN** ادمین IsPromptPublic را تغییر دهد
- **THEN** اعلان با category `open_prompt` برای creator ارسال شود

#### Scenario: تایید برداشت
- **WHEN** ادمین برداشت را تایید کند
- **THEN** اعلان با category `withdrawal` برای creator ارسال شود

#### Scenario: رد برداشت
- **WHEN** ادمین برداشت را رد کند
- **THEN** اعلان با دلیل رد و category `withdrawal` برای creator ارسال شود

#### Scenario: پیام از ادمین
- **WHEN** ادمین پیام بفرستد (Detail یا Thread)
- **THEN** اعلان با category `general` برای creator ارسال شود

### Requirement: تریگرهای Admin → User
سیستم SHALL هنگام اقدامات ادمین روی حساب کاربر، اعلان برای آن کاربر ارسال کند.

#### Scenario: مسدود/فعال‌سازی
- **WHEN** ادمین حساب کاربر را مسدود یا فعال کند
- **THEN** اعلان با category `general` برای کاربر ارسال شود

#### Scenario: تعدیل اعتبار
- **WHEN** ادمین اعتبار کاربر را بصورت دستی تنظیم کند
- **THEN** اعلان با مقدار و دلیل، category `general` برای کاربر ارسال شود

#### Scenario: تغییر نقش
- **WHEN** ادمین نقش کاربر را تغییر دهد
- **THEN** اعلان با نام نقش جدید، category `general` برای کاربر ارسال شود

#### Scenario: استرداد پرداخت
- **WHEN** ادمین پرداختی را استرداد کند
- **THEN** اعلان با مقدار اعتبار و شناسه پرداخت، category `general` برای کاربر ارسال شود

#### Scenario: تایید/حذف نظر توسط ادمین
- **WHEN** ادمین نظر کاربر را تایید یا حذف کند
- **THEN** اعلان با category `review` برای صاحب نظر ارسال شود

### Requirement: تریگرهای نظرات (Creator و User)
سیستم SHALL برای رویدادهای مرتبط با نظرات اعلان مناسب ارسال کند.

#### Scenario: نظر جدید کاربر
- **WHEN** کاربر نظر جدیدی برای ابزار ثبت کند
- **THEN** اعلان با category `review` برای creator آن ابزار ارسال شود

#### Scenario: تایید/رد نظر توسط سازنده
- **WHEN** سازنده نظر کاربر را تایید یا رد کند
- **THEN** اعلان با category `review` برای صاحب نظر ارسال شود

### Requirement: تریگر خرید اعتبار موفق
سیستم SHALL پس از تایید موفق پرداخت ZarinPal، اعلان برای کاربر ارسال کند.

#### Scenario: پرداخت موفق
- **WHEN** `VerifyPaymentAsync` کد 100 یا 101 از ZarinPal دریافت کند
- **THEN** اعلان «خرید اعتبار موفق: N اعتبار» با category `general` برای کاربر ارسال شود
