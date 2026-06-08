# Services و Interfaces

## لیست کامل سرویس‌ها

---

### IAppService / AppService
مسئول تمام عملیات مرتبط با AiApp:

- GetPublishedAppsAsync — لیست ابزارهای فعال با فیلتر و صفحه‌بندی
- GetAppBySlugAsync — جزئیات یک ابزار برای نمایش عمومی
- GetAppsByCreatorAsync — ابزارهای یک سازنده (برای پنل Creator)
- CreateAppAsync — ساخت ابزار جدید با Status = Draft
- UpdateAppAsync — ویرایش (فقط اگر Draft یا Suspended باشد)
- SubmitForReviewAsync — ارسال برای بررسی ادمین
- UpdateStatusAsync — تغییر وضعیت (فقط ادمین)
- DeleteAppAsync — حذف نرم (Soft Delete)
- IncrementExecutionCountAsync — افزایش تعداد اجرا
- RecalculateRatingAsync — محاسبه مجدد میانگین امتیاز

---

### IExecutionService / ExecutionService
قلب پروژه — مسئول اجرای امن پرامپت:

- ExecuteAsync — اجرای کامل از validation تا توزیع درآمد
- GetExecutionAsync — خواندن یک اجرا (فقط توسط خود کاربر)
- GetUserExecutionsAsync — تاریخچه اجراهای کاربر
- GetAppExecutionsAsync — آمار اجراها برای صاحب ابزار
- RefundExecutionAsync — بازگشت اعتبار

---

### ICreditService / CreditService
مدیریت کیف‌پول اعتباری:

- GetBalanceAsync — موجودی اعتبار
- HasEnoughCreditsAsync — بررسی کافی بودن موجودی
- DeductCreditsAsync — کم کردن اعتبار با ثبت تراکنش
- AddCreditsAsync — اضافه کردن اعتبار با ثبت تراکنش
- DistributeEarningsAsync — توزیع درآمد بین سازنده و پلتفرم
- GetTransactionHistoryAsync — تاریخچه تراکنش‌ها

---

### IPaymentService / PaymentService
مدیریت پرداخت ZarinPal:

- GetActivePackagesAsync — لیست بسته‌های فعال
- InitiatePaymentAsync — شروع پرداخت و دریافت Authority
- VerifyPaymentAsync — تایید پرداخت و افزودن اعتبار
- GetPaymentHistoryAsync — تاریخچه پرداخت‌های کاربر

---

### IAiService / AiService
ارتباط با OpenRouter:

- RunTextPromptAsync — اجرای پرامپت متنی
- RunImagePromptAsync — اجرای پرامپت تصویری
- RunVideoPromptAsync — شروع تولید ویدیو (async)
- CheckVideoStatusAsync — بررسی وضعیت تولید ویدیو
- RunAudioPromptAsync — اجرای تولید صدا
- GetModelCapabilitiesAsync — قابلیت‌های یک مدل

---

### IAiProviderService / AiProviderService
مدیریت مدل‌های AI:

- GetAllProvidersAsync — همه providers
- GetAllModelsAsync — همه مدل‌ها با فیلتر capability
- GetModelsForOutputTypeAsync — مدل‌های مناسب برای یک OutputType
- GetActiveModelByIdAsync — خواندن یک مدل فعال
- GetApiKeyAsync — خواندن و رمزگشایی API Key یک provider

---

### IEncryptionService / EncryptionService
رمزنگاری داده‌های حساس:

- Encrypt — رمزنگاری با AES-256
- Decrypt — رمزگشایی
- HashPassword — برای مقادیر یکطرفه

---

### ISettingService / SettingService
خواندن و نوشتن SystemSetting:

- GetValueAsync — خواندن یک تنظیم با Key
- GetGroupAsync — خواندن همه تنظیمات یک Group
- SetValueAsync — ذخیره / بروزرسانی یک تنظیم
- GetZarinpalConfigAsync — برگرداندن تنظیمات ZarinPal بصورت typed
- GetAiConfigAsync — برگرداندن تنظیمات AI بصورت typed

---

### IStorageService / LocalStorageService
ذخیره فایل‌های خروجی (تصویر، ویدیو):

- SaveFromUrlAsync — دانلود و ذخیره فایل از URL
- GetPublicUrl — برگرداندن URL عمومی فایل
- DeleteAsync — حذف فایل

در نسخه اول: ذخیره روی سرور در wwwroot/uploads
در نسخه بعدی: قابل swap با Azure Blob یا S3

---

### ISlugService / SlugService
- GenerateSlug — تبدیل عنوان به Slug
- EnsureUniqueAsync — اضافه کردن پسوند عددی در صورت تکراری بودن

---

### IReviewService / ReviewService
- GetAppReviewsAsync — نظرات یک ابزار
- AddReviewAsync — ثبت نظر (فقط اگر حداقل یک بار اجرا کرده)
- HasUserReviewedAsync — بررسی اینکه کاربر قبلاً نظر داده یا نه

---

### IWithdrawalService / WithdrawalService
- RequestWithdrawalAsync — ثبت درخواست برداشت توسط سازنده
- GetCreatorRequestsAsync — تاریخچه درخواست‌های سازنده
- GetPendingRequestsAsync — لیست درخواست‌های در انتظار (ادمین)
- ProcessRequestAsync — تایید یا رد درخواست توسط ادمین

---

## نکات معماری

### Dependency Injection
همه سرویس‌ها به صورت Scoped ثبت می‌شوند مگر EncryptionService که Singleton است.

### Validation
- ورودی‌های فرم با Data Annotations روی ViewModel
- منطق کسب‌وکار داخل سرویس‌ها
- PageModel فقط routing و mapping انجام می‌دهد

### Error Handling
- سرویس‌ها به جای Exception، یک Result<T> object برمی‌گردانند که شامل IsSuccess، Error، و Value است
- PageModel بر اساس Result تصمیم می‌گیرد

### Transactions
- عملیاتی مثل DeductCredits + Execution + DistributeEarnings باید در یک Database Transaction باشند
