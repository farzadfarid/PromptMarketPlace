# Change: سیستم اعلان درون‌برنامه‌ای (Notification Bell + Toast)

## Why

کاربران، سازندگان و ادمین‌ها هیچ کانال اطلاع‌رسانی درون‌برنامه‌ای ندارند. وقتی ادمین ابزاری را تایید یا رد می‌کند، وقتی برداشت پرداخت می‌شود، یا وقتی کاربری نظر می‌دهد، هیچ اعلانی به طرف مقابل نمی‌رسد. یک سیستم اعلان سبک با polling (بدون WebSocket)، به‌علاوه toast notification برای رویدادهای real-time، این شکاف ارتباطی را پر می‌کند.

## What Changes

### پایه (دیتابیس + سرویس)
- entity جدید `Notification`: Id, UserId, Title, Message?, Link?, Category, IsRead, CreatedAt
- فیلد `Category` (string): `app_review` | `app_status` | `open_prompt` | `review` | `withdrawal` | `general`
- `INotificationService` با متدهای: CreateAsync، CreateForAdminsAsync، GetUnreadCountAsync، GetRecentAsync، MarkAllReadAsync
- Migration: `AddNotifications`
- ثبت سرویس در `Program.cs` به صورت Scoped

### API
- `GET /api/notifications?handler=unread` → `{count: N}`
- `GET /api/notifications?handler=recent` → JSON آرایه ۱۰ اعلان اخیر (با id، title، message، link، category، isRead، createdAt)
- `POST /api/notifications?handler=markAllRead` → `{ok: true}`

### UI — زنگ و Dropdown
- زنگ 🔔 در topbar ادمین و سازنده با badge تعداد خوانده‌نشده (بنفش/نارنجی وقتی دارد)
- SSR اولیه: layout مستقیماً از `INotificationService` ۱۰ اعلان و تعداد می‌خواند
- Dropdown گلاس‌مورفیسم: آیکون دسته‌بندی رنگی + عنوان + پیام + زمان نسبی فارسی
- دکمه «همه خوانده شد» در dropdown
- رنگ‌بندی category: آبی=app_review، سبز=app_status، بنفش=open_prompt، طلایی=review، زمرد=withdrawal

### UI — Toast Notification
- کانتینر `position: fixed` در گوشه چپ‌بالا (زیر topbar)
- Slide از چپ با spring animation هنگام ورود
- نوار رنگی accent بر اساس category در بالا
- آیکون دسته، عنوان، پیام کوتاه
- Progress bar پایین که در ۶ ثانیه خالی می‌شود (auto-dismiss)
- Hover روی toast → pause؛ mouseleave → resume با ۱.۵ ثانیه باقی‌مانده
- Stack حداکثر ۳ toast همزمان
- فقط برای رویدادهای **جدید** (بعد از بارگذاری صفحه) نمایش داده می‌شود

### Polling
- polling هر ۳۰ ثانیه از `GET /api/notifications?handler=unread`
- Poll اول: فقط baseline مقداردهی اولیه می‌کند (toast نشان نمی‌دهد)
- Poll‌های بعدی: اگر count بیشتر از قبل شد، recent را fetch کرده، برای آیتم‌های جدید (id > lastSeenMaxId) toast نشان می‌دهد

### ۲۱ تریگر اعلان

**Creator → Admin:**
- T1: ارسال ابزار برای بررسی (`app_review`)
- T2: درخواست پرامپت باز (`open_prompt`)
- T3: لغو درخواست پرامپت باز (`open_prompt`)
- T4: درخواست برداشت (`withdrawal`)
- T8a: پیام جدید از سازنده — thread جدید (`general`)
- T8b: پاسخ سازنده در thread موجود (`general`)

**Admin → Creator:**
- T5: تغییر وضعیت ابزار Active/Suspended (`app_status`)
- T6: تایید پرامپت باز (`open_prompt`)
- T6b: لغو پرامپت باز توسط ادمین (`open_prompt`)
- T7: تایید برداشت (`withdrawal`)
- T8: رد برداشت (`withdrawal`)
- T7a: پیام جدید از ادمین به سازنده (`general`)
- T7b: پاسخ ادمین در thread موجود (`general`)
- N1: تایید ابزار از صفحه Review (`app_review`)
- N2: رد ابزار از صفحه Review با دلیل (`app_review`)

**User → Creator:**
- T9: نظر جدید کاربر (`review`)

**Admin → User:**
- N3: مسدود/فعال‌سازی حساب (`general`)
- N4: تعدیل اعتبار دستی (`general`)
- N5: تغییر نقش کاربر (`general`)
- N6: استرداد پرداخت (`general`)
- N9/N10: تایید/حذف نظر (`review`)

**Creator → User:**
- N11: تایید/رد نظر توسط سازنده (`review`)

**System → User:**
- N12: خرید اعتبار موفق (ZarinPal verify) (`general`)

## Impact

- Affected specs: `notifications`
- Affected code:
  - `Models/Domain/Notification.cs` — entity جدید
  - `Data/ApplicationDbContext.cs` — `DbSet<Notification>`
  - `Migrations/` — `AddNotifications`
  - `Services/Interfaces/INotificationService.cs` — interface
  - `Services/NotificationService.cs` — پیاده‌سازی
  - `Services/PaymentService.cs` — تریگر N12
  - `Program.cs` — ثبت سرویس
  - `Pages/Api/Notifications.cshtml.cs` — سه handler
  - `Areas/Admin/Pages/Shared/_Layout.cshtml` — زنگ در topbar
  - `Areas/Creator/Pages/Shared/_Layout.cshtml` — زنگ در topbar
  - `Areas/Admin/Pages/Apps/Review.cshtml.cs` — N1, N2
  - `Areas/Admin/Pages/Apps/Detail.cshtml.cs` — T5, T6, T7a, N9, N10
  - `Areas/Admin/Pages/Messages/Thread.cshtml.cs` — T7b
  - `Areas/Admin/Pages/Withdrawals/Index.cshtml.cs` — T7, T8
  - `Areas/Admin/Pages/Users/Detail.cshtml.cs` — N3, N4, N5
  - `Areas/Admin/Pages/Payments/Index.cshtml.cs` — N6
  - `Areas/Admin/Pages/Reviews/Index.cshtml.cs` — N9, N10
  - `Areas/Creator/Pages/Apps/Submit.cshtml.cs` — T1
  - `Areas/Creator/Pages/Apps/Edit.cshtml.cs` — T2, T3
  - `Areas/Creator/Pages/Earnings/Index.cshtml.cs` — T4
  - `Areas/Creator/Pages/Messages/Index.cshtml.cs` — T8a
  - `Areas/Creator/Pages/Messages/Thread.cshtml.cs` — T8b
  - `Areas/Creator/Pages/Reviews/Index.cshtml.cs` — N11
  - `Pages/App/Detail.cshtml.cs` — T9
  - `wwwroot/js/notification-bell.js` — polling + dropdown + toast
  - `wwwroot/css/notification-bell.css` — استایل زنگ + dropdown + toast
