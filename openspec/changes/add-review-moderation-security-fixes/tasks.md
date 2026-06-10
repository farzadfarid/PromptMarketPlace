## 1. Domain Model
- [x] 1.1 افزودن `IsApproved`, `CreatorReply`, `RepliedAt` به `AppReview`
- [x] 1.2 اجرای migration `AddReviewApproval`

## 2. Review Service
- [x] 2.1 `GetAppReviewsAsync` — فقط approved نمایش داده شود
- [x] 2.2 `AddReviewAsync` — `IsApproved = false` پیش‌فرض
- [x] 2.3 `ApproveAsync(reviewId, creatorProfileId?)` — سازنده یا ادمین
- [x] 2.4 `RejectAsync(reviewId, creatorProfileId?)` — حذف نظر
- [x] 2.5 `ReplyAsync(reviewId, creatorProfileId, reply)` — فقط سازنده
- [x] 2.6 `GetPendingReviewsForCreatorAsync` + `GetAllReviewsForCreatorAsync` + `GetPendingCountForCreatorAsync`
- [x] 2.7 `RecalculateRatingAsync` — فقط approved را بشمارد

## 3. Star Rating Fix
- [x] 3.1 جایگزینی radio input‌های hidden با `<input type="hidden" name="rating">`
- [x] 3.2 JS: مقدار hidden input با کلیک روی ستاره ست می‌شود
- [x] 3.3 JS: validation قبل از submit — اگر ستاره انتخاب نشده باشد خطا نشان داده می‌شود

## 4. Public Detail Page
- [x] 4.1 پیام «در انتظار تایید سازنده» بعد از ثبت نظر
- [x] 4.2 نمایش پاسخ سازنده در لیست نظرات

## 5. Creator Reviews Page (جدید)
- [x] 5.1 ساختن `Areas/Creator/Pages/Reviews/Index.cshtml[.cs]`
- [x] 5.2 لیست نظرات با فیلتر pending/all
- [x] 5.3 دکمه تایید + modal رد کردن (Bootstrap، بدون confirm مرورگر)
- [x] 5.4 فرم پاسخ برای نظرات تایید شده
- [x] 5.5 افزودن آیتم «نظرات» با badge pending به sidebar

## 6. Admin Reviews Page
- [x] 6.1 ستون وضعیت (منتشر/در انتظار)
- [x] 6.2 دکمه تایید برای نظرات pending
- [x] 6.3 فیلتر وضعیت
- [x] 6.4 نمایش تعداد pending در header

## 7. Admin App Detail
- [x] 7.1 نمایش امتیاز عددی در کنار ستاره‌ها
- [x] 7.2 badge وضعیت (منتشر/در انتظار) برای هر نظر
- [x] 7.3 دکمه تایید/حذف برای هر نظر

## 8. User Blocking Security Fix
- [x] 8.1 `Login.cshtml.cs` — چک `IsActive` بعد از `result.Succeeded`؛ sign out + پیام خطا
- [x] 8.2 `Detail.cshtml.cs` — `UpdateSecurityStampAsync` هنگام مسدودسازی
- [x] 8.3 `Program.cs` — `SecurityStampValidatorOptions.ValidationInterval = 1 minute`
