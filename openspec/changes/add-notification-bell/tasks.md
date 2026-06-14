# Tasks: add-notification-bell

## ۱. دیتابیس و سرویس
- [x] T01 — Entity `Notification` + `DbSet<Notification>` در `ApplicationDbContext`
- [x] T02 — Migration: `AddNotifications` (`dotnet ef migrations add AddNotifications && dotnet ef database update`)
- [x] T03 — `INotificationService` + `NotificationService` (Create / CreateForAdmins / GetUnreadCount / GetRecent / MarkAllRead)
- [x] T04 — ثبت سرویس Scoped در `Program.cs`

## ۲. API
- [x] T05 — `Pages/Api/Notifications.cshtml.cs` — handler: unread → `{count}` | recent → JSON list | markAllRead → `{ok:true}`

## ۳. UI — زنگ و Dropdown
- [x] T06 — `wwwroot/css/notification-bell.css`: گلاس‌مورفیسم، badge با pulse، dropdown با انیمیشن، رنگ‌بندی category
- [x] T07 — `wwwroot/js/notification-bell.js`: polling 30s، dropdown، mark-all-read، زمان نسبی فارسی
- [x] T08 — زنگ با SSR اولیه در `Areas/Admin/Pages/Shared/_Layout.cshtml`
- [x] T09 — زنگ با SSR اولیه در `Areas/Creator/Pages/Shared/_Layout.cshtml`

## ۴. UI — Toast Notification
- [x] T10 — CSS toast در `notification-bell.css`: slide از چپ، accent bar، progress bar، hover-pause
- [x] T11 — JS toast: `showToast`، `dismissToast`، `getToastContainer`، `initialized` flag، `lastSeenMaxId`

## ۵. تریگرهای Creator → Admin
- [x] T12 — `Creator/Apps/Submit.cshtml.cs`: ارسال ابزار → admins (`app_review`)
- [x] T13 — `Creator/Apps/Edit.cshtml.cs`: درخواست/لغو پرامپت باز → admins (`open_prompt`)
- [x] T14 — `Creator/Earnings/Index.cshtml.cs`: درخواست برداشت → admins (`withdrawal`)
- [x] T15 — `Creator/Messages/Index.cshtml.cs`: ایجاد thread پیام جدید → admins (`general`)
- [x] T16 — `Creator/Messages/Thread.cshtml.cs`: پاسخ سازنده در thread → admins (`general`)

## ۶. تریگرهای Admin → Creator
- [x] T17 — `Admin/Apps/Review.cshtml.cs`: تایید ابزار → creator (`app_review`)
- [x] T18 — `Admin/Apps/Review.cshtml.cs`: رد ابزار با دلیل → creator (`app_review`)
- [x] T19 — `Admin/Apps/Detail.cshtml.cs`: تغییر وضعیت Active/Suspended → creator (`app_status`)
- [x] T20 — `Admin/Apps/Detail.cshtml.cs`: تایید/لغو پرامپت باز → creator (`open_prompt`)
- [x] T21 — `Admin/Apps/Detail.cshtml.cs`: پیام در صفحه Detail → creator (`general`)
- [x] T22 — `Admin/Messages/Thread.cshtml.cs`: پاسخ ادمین در thread → creator (`general`)
- [x] T23 — `Admin/Withdrawals/Index.cshtml.cs`: تایید/رد برداشت → creator (`withdrawal`)

## ۷. تریگرهای Admin → User
- [x] T24 — `Admin/Users/Detail.cshtml.cs`: مسدود/فعال‌سازی حساب → user (`general`)
- [x] T25 — `Admin/Users/Detail.cshtml.cs`: تعدیل اعتبار دستی → user (`general`)
- [x] T26 — `Admin/Users/Detail.cshtml.cs`: تغییر نقش → user (`general`)
- [x] T27 — `Admin/Payments/Index.cshtml.cs`: استرداد پرداخت → user (`general`)
- [x] T28 — `Admin/Reviews/Index.cshtml.cs`: تایید/حذف نظر → user (`review`)

## ۸. تریگرهای Creator → User
- [x] T29 — `Creator/Reviews/Index.cshtml.cs`: تایید/رد نظر توسط سازنده → user (`review`)

## ۹. تریگرهای User → Creator
- [x] T30 — `Pages/App/Detail.cshtml.cs`: نظر جدید کاربر → creator (`review`)

## ۱۰. تریگرهای System → User
- [x] T31 — `Services/PaymentService.cs`: خرید اعتبار موفق → user (`general`)

## ۱۱. رفع باگ‌ها
- [x] T32 — رفع لینک اعلان: فرمت صحیح `/Admin/Apps/{id}` (Razor Pages route)
- [x] T33 — رفع dropdown برش‌خورده در RTL: `left: 0; right: auto; transform: translateY(-6px)`
