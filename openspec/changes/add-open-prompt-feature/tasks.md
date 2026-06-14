# Tasks: add-open-prompt-feature

## ۱. دیتابیس
- [x] T01 — اضافه کردن `IsPromptPublicRequested` و `IsPromptPublic` (هر دو bool, default false) به `AiApp`
- [x] T02 — Migration: `AddOpenPromptFields` (`dotnet ef migrations add AddOpenPromptFields && dotnet ef database update`)

## ۲. سرویس لایه داده
- [x] T03 — اضافه کردن فیلد `OnlyOpenPrompt` به `AppFilterDto`
- [x] T04 — اضافه کردن فیلتر `if (filter.OnlyOpenPrompt) query = query.Where(a => a.IsPromptPublic)` در `AppService.GetPublishedAppsAsync`

## ۳. صفحه ویرایش سازنده
- [x] T05 — `OnPostTogglePromptRequestAsync` در `Creator/Apps/Edit.cshtml.cs` — toggle IsPromptPublicRequested؛ اگر لغو شود IsPromptPublic هم false شود
- [x] T06 — property `OpenPromptStatus` در PageModel برای نمایش وضعیت
- [x] T07 — کارت «پرامپت باز» در `Creator/Apps/Edit.cshtml` با toggle switch و بدج وضعیت سه‌گانه

## ۴. صفحه جزئیات ادمین
- [x] T08 — `OnPostToggleOpenPromptAsync` در `Admin/Apps/Detail.cshtml.cs` — approve/revoke + AuditLog
- [x] T09 — بخش «پرامپت باز» در `Admin/Apps/Detail.cshtml` با نمایش وضعیت درخواست + دکمه Approve/Revoke

## ۵. صفحه عمومی (Public App Detail)
- [x] T10 — decrypt پرامپت در `Pages/App/Detail.cshtml.cs` (فقط اگر `IsPromptPublic = true`)
- [x] T11 — بلاک «پرامپت باز» در `Pages/App/Detail.cshtml`: متن پرامپت + دکمه کپی + توضیح هزینه + بدج هدر
- [x] T12 — JS کپی پرامپت در `wwwroot/js/pages/public/Detail.js`

## ۶. برچسب در کارت ابزار
- [x] T13 — بدج «پرامپت باز» در `Pages/Shared/_AppCard.cshtml` (فقط وقتی `IsPromptPublic = true`)
- [x] T14 — استایل `.app-card__open-badge` در `wwwroot/css/site.css` (absolute روی thumbnail، پس‌زمینه بنفش gradient)

## ۷. فیلتر صفحه Explore
- [x] T15 — `[BindProperty(SupportsGet = true)] public bool OnlyOpenPrompt` در `Pages/Explore.cshtml.cs`
- [x] T16 — hidden input `OnlyOpenPrompt` در فرم‌های search و sort در `Pages/Explore.cshtml`
- [x] T17 — چک‌باکس «فقط پرامپت باز» در sidebar فیلترها در `Pages/Explore.cshtml`
- [x] T18 — `asp-route-OnlyOpenPrompt` در تمام لینک‌های pagination در `Pages/Explore.cshtml`
