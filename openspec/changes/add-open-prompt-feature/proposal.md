# Change: قابلیت پرامپت باز (Open Prompt)

## Why

سازندگان ممکن است بخواهند پرامپت ابزار خود را به صورت شفاف در اختیار کاربران قرار دهند تا اعتماد ایجاد کنند. اما نمایش پرامپت باید با تایید ادمین انجام شود تا محتوای نامناسب یا مشکل‌دار فیلتر شود. کاربران عمومی می‌توانند پرامپت را مشاهده و کپی کنند، اما اجرای ابزار همچنان هزینه کردیت دارد. برچسب مشخص و فیلتر در صفحه Explore به کاربران کمک می‌کند این ابزارها را سریع‌تر پیدا کنند.

## What Changes

### گردش کار و دسترسی‌ها
- فیلد `IsPromptPublicRequested` (bool) به `AiApp` — سازنده درخواست می‌دهد
- فیلد `IsPromptPublic` (bool) به `AiApp` — ادمین تایید می‌کند
- **گردش کار:** سازنده درخواست می‌دهد → ادمین تایید یا رد می‌کند
- **دسترسی مستقیم ادمین:** ادمین می‌تواند بدون درخواست سازنده نیز مستقیماً toggle کند
- سازنده فقط روی ابزارهای Published (Active) می‌تواند درخواست دهد — بدون تعلیق ابزار
- Migration: `AddOpenPromptFields`

### صفحه عمومی ابزار (App Detail)
- اگر `IsPromptPublic = true`: بلاک «پرامپت باز» با متن رمزگشایی‌شده + دکمه کپی + توضیح هزینه اجرا
- بدج «پرامپت باز» در هدر صفحه

### صفحه ویرایش سازنده (Creator Edit)
- کارت «پرامپت باز»: toggle درخواست + نمایش وضعیت (در انتظار / تایید شده / تایید نشده)

### صفحه جزئیات ادمین (Admin Apps/Detail)
- بخش «پرامپت باز»: وضعیت درخواست + دکمه Approve/Revoke
- AuditLog برای اقدام ادمین

### برچسب در کارت ابزار (App Card Badge)
- بدج «پرامپت باز» با آیکون قفل-باز روی thumbnail کارت‌های ابزار در Explore و سایر لیست‌ها
- فقط وقتی `IsPromptPublic = true`

### فیلتر در صفحه Explore
- چک‌باکس «فقط پرامپت باز» در sidebar فیلترها
- `OnlyOpenPrompt` به عنوان query string در URL و pagination links
- فیلتر `if (filter.OnlyOpenPrompt) query = query.Where(a => a.IsPromptPublic)` در `AppService`

## Impact

- Affected specs: `open-prompt`
- Affected code:
  - `Models/Domain/AiApp.cs` — دو فیلد bool جدید
  - `Models/AppFilterDto.cs` — فیلد `OnlyOpenPrompt`
  - `Migrations/` — `AddOpenPromptFields`
  - `Services/AppService.cs` — فیلتر `OnlyOpenPrompt` در `GetPublishedAppsAsync`
  - `Areas/Creator/Pages/Apps/Edit.cshtml` + `.cshtml.cs` — toggle درخواست + وضعیت
  - `Areas/Admin/Pages/Apps/Detail.cshtml` + `.cshtml.cs` — approve/revoke handler + AuditLog
  - `Pages/App/Detail.cshtml` + `.cshtml.cs` — نمایش پرامپت + بدج هدر
  - `Pages/Shared/_AppCard.cshtml` — بدج «پرامپت باز» روی thumbnail
  - `Pages/Explore.cshtml` + `.cshtml.cs` — فیلتر + pagination links
  - `wwwroot/css/site.css` — استایل `.app-card__open-badge`
  - `wwwroot/js/pages/public/Detail.js` — JS کپی پرامپت
