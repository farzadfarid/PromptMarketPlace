# Change: سیستم پیام‌رسانی آفلاین ادمین–سازنده

## Why
ادمین و سازنده راهی برای ارتباط مستقیم درباره ابزارها یا موضوعات عمومی (مثل پرداخت) نداشتند. بررسی، رد، یا درخواست اصلاح ابزار نیاز به کانال ارتباطی رسمی دارد.

## What Changes
- مدل‌های `MessageThread` و `ThreadMessage` اضافه شدند
- سرویس `IMessageService` / `MessageService` پیاده‌سازی شد
- صفحات Admin: لیست مکالمات + صفحه chat
- صفحات Creator: inbox + صفحه chat + modal پیام جدید
- دکمه "ارسال پیام" به صفحه جزئیات ابزار ادمین اضافه شد
- آیتم "پیام‌ها" با unread badge به سایدبار هر دو پنل اضافه شد

## Impact
- Affected specs: `admin-creator-messaging` (جدید)
- Affected code:
  - `Models/Domain/MessageThread.cs` (جدید)
  - `Models/Domain/ThreadMessage.cs` (جدید)
  - `Models/Domain/CreatorProfile.cs` (اضافه شدن navigation)
  - `Data/ApplicationDbContext.cs`
  - `Services/Interfaces/IMessageService.cs` (جدید)
  - `Services/MessageService.cs` (جدید)
  - `Program.cs`
  - `Areas/Admin/Pages/Messages/Index.cshtml` + `.cs` (جدید)
  - `Areas/Admin/Pages/Messages/Thread.cshtml` + `.cs` (جدید)
  - `Areas/Creator/Pages/Messages/Index.cshtml` + `.cs` (جدید)
  - `Areas/Creator/Pages/Messages/Thread.cshtml` + `.cs` (جدید)
  - `Areas/Admin/Pages/Apps/Detail.cshtml` + `.cs`
  - `Areas/Admin/Pages/Shared/_Layout.cshtml`
  - `Areas/Creator/Pages/Shared/_Layout.cshtml`

## Status
پیاده‌سازی کامل شده. Migration اعمال شده.
