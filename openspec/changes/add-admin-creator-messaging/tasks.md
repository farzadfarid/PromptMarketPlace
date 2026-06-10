## 1. Domain و Database

- [x] 1.1 ایجاد `MessageThread` (Id, CreatorProfileId FK, AppId FK nullable, Subject, CreatedAt, LastMessageAt)
- [x] 1.2 ایجاد `ThreadMessage` (Id, ThreadId FK, IsFromAdmin, Content, SentAt, IsRead)
- [x] 1.3 اضافه کردن navigation `MessageThreads` به `CreatorProfile`
- [x] 1.4 اضافه کردن `DbSet` های جدید به `ApplicationDbContext`
- [x] 1.5 اجرای migration (`dotnet ef migrations add AddMessaging` + `database update`)

## 2. Service

- [x] 2.1 تعریف `IMessageService` با متدها: StartThread, GetThread, GetAdminThreads, GetCreatorThreads, Send, MarkRead, UnreadCount
- [x] 2.2 پیاده‌سازی `MessageService`
- [x] 2.3 ثبت سرویس در `Program.cs`

## 3. Admin Pages

- [x] 3.1 `Messages/Index.cshtml` — لیست مکالمات با unread badge و app tag
- [x] 3.2 `Messages/Thread.cshtml` — chat bubble UI با read receipts و reply form
- [x] 3.3 دکمه "ارسال پیام" به Admin App Detail اضافه شد (modal با پیش‌پر موضوع)
- [x] 3.4 handler `OnPostSendMessageAsync` در Detail.cshtml.cs
- [x] 3.5 آیتم "پیام‌ها" با unread badge به Admin layout اضافه شد

## 4. Creator Pages

- [x] 4.1 `Messages/Index.cshtml` — inbox با modal "پیام جدید"
- [x] 4.2 `Messages/Thread.cshtml` — chat UI (پیام‌های خود راست، ادمین چپ) با read receipts
- [x] 4.3 handler `OnPostNewThreadAsync` برای ایجاد thread عمومی
- [x] 4.4 آیتم "پیام‌ها" با unread badge به Creator layout اضافه شد
