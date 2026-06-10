# Change: Review Moderation + User Blocking Security Fix

## Why

نظرات کاربران بلافاصله بعد از ثبت نمایش داده می‌شدند (بدون تایید سازنده)، ستاره‌بندی فرم نظر به دلیل رفتار مرورگر با radio hidden مقدار اشتباه ذخیره می‌کرد، و مسدودسازی کاربر از ورود جلوگیری نمی‌کرد.

## What Changes

- **BREAKING** فیلد `IsApproved` (پیش‌فرض false) و `CreatorReply` / `RepliedAt` به مدل `AppReview` اضافه شد — migration لازم است
- نظرات تا تایید سازنده برای عموم نمایش داده نمی‌شوند
- سازنده می‌تواند نظرات ابزارهای خود را تایید، رد یا پاسخ دهد
- ادمین می‌تواند نظرات را تایید یا حذف کند
- امتیاز میانگین ابزار فقط از نظرات تایید شده محاسبه می‌شود
- فرم ستاره‌بندی با hidden input جایگزین radio button های hidden شد — مقدار دقیق ذخیره می‌شود
- Login چک `IsActive` را انجام می‌دهد — کاربر مسدود قادر به ورود نیست
- مسدودسازی کاربر SecurityStamp را update می‌کند — تمام session‌های فعال باطل می‌شوند
- `SecurityStampValidatorOptions.ValidationInterval` به ۱ دقیقه کاهش یافت

## Impact

- Affected specs: `01-domain-models`, `05-services-interfaces`, `06-public-pages`, `07-creator-area`, `09-admin-area`
- Affected code:
  - `Models/Domain/AppReview.cs` — فیلدهای جدید
  - `Services/ReviewService.cs` + `IReviewService.cs` — approve/reject/reply
  - `Services/AppService.cs` — RecalculateRatingAsync فقط approved را می‌شمارد
  - `Pages/Auth/Login.cshtml.cs` — چک IsActive
  - `Areas/Admin/Pages/Users/Detail.cshtml.cs` — UpdateSecurityStampAsync
  - `Areas/Creator/Pages/Reviews/Index.cshtml[.cs]` — صفحه جدید
  - `Areas/Admin/Pages/Reviews/Index.cshtml[.cs]` — approve + status
  - `Program.cs` — ValidationInterval
