## ۱. مودال نمونه خروجی
- [x] 1.1 اضافه کردن data attributes به showcase items در `Pages/App/Detail.cshtml` با Razor auto-encode
- [x] 1.2 اضافه کردن `#showcaseModal` Bootstrap Modal به انتهای بخش sidebar
- [x] 1.3 پیاده‌سازی click handler در `wwwroot/js/pages/public/Detail.js` برای پر کردن modal

## ۲. آمار مصرف توکن واقعی
- [x] 2.1 اضافه کردن `AvgTokensAll`, `AvgTokens30d`, `IsUnprofitable` و ثابت `TokensPerCredit=1000` به `Admin/Apps/Detail.cshtml.cs`
- [x] 2.2 اضافه کردن query مصرف توکن در `OnGetAsync` ادمین (فیلتر: Completed + TokensUsed!=null)
- [x] 2.3 اضافه کردن کارت "مصرف توکن واقعی" در `Admin/Apps/Detail.cshtml` (هزینه اعتبار همیشه نمایان)
- [x] 2.4 اضافه کردن همان properties به `Creator/Apps/Edit.cshtml.cs`
- [x] 2.5 اضافه کردن query مصرف توکن در `OnGetAsync` سازنده
- [x] 2.6 اضافه کردن نوار آمار در `Creator/Apps/Edit.cshtml` قبل از wizard steps

## ۳. ماشین حساب تخمین توکن
- [x] 3.1 اضافه کردن computed properties `PromptCharCount` و `SystemContextCharCount` به `Admin/Apps/Detail.cshtml.cs`
- [x] 3.2 رفع double-decrypt در `Creator/Apps/Edit.cshtml.cs`: یک بار decrypt، استفاده برای هم `Form.NewPrompt` هم `PromptCharCount`
- [x] 3.3 اضافه کردن `PromptCharCount`, `SystemContextCharCount` به `Creator/Apps/Edit.cshtml.cs`
- [x] 3.4 اضافه کردن کارت Collapsible calculator در `Admin/Apps/Detail.cshtml` با `data-prompt`, `data-sys`, `data-fields`
- [x] 3.5 اضافه کردن کارت Collapsible calculator در `Creator/Apps/Edit.cshtml`
- [x] 3.6 پیاده‌سازی `updateAdminCalc()` در JS ادمین
- [x] 3.7 پیاده‌سازی `updateCreatorCalc()` در JS سازنده

## ۴. راهنمای تعاملی (Help Popovers)
- [x] 4.1 اضافه کردن آیکون `?` با کلاس `help-popover-btn` و `data-help-id` به header کارت "مصرف توکن واقعی" (ادمین)
- [x] 4.2 اضافه کردن آیکون `?` به header کارت "ماشین حساب" (ادمین)
- [x] 4.3 اضافه کردن آیکون `?` به نوار آمار و کارت calculator (سازنده)
- [x] 4.4 پیاده‌سازی `helpData` object با محتوای کامل فارسی برای هر دو بخش
- [x] 4.5 init Bootstrap Popover با `html:true, trigger:'click', customClass:'help-popover'`
- [x] 4.6 رفع باگ ادمین: انتقال script از inline body به `@section Scripts` (bootstrap.js قبل از script لازم است)
- [x] 4.7 اضافه کردن CSS `.help-popover`, `.help-popover-btn` به `wwwroot/css/site.css`
- [x] 4.8 document click handler برای بستن popover هنگام کلیک بیرون
