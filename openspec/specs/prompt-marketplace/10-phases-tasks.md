# فازبندی اجرایی پروژه

---

## فاز ۰ — زیرساخت (۳ تا ۵ روز)

**هدف:** پروژه آماده کدنویسی شود

- ساخت پروژه ASP.NET Core 9 با Razor Pages
- نصب و کانفیگ پکیج‌ها (EF Core, Identity, Serilog)
- تعریف ساختار فولدرها
- تنظیم appsettings.json
- راه‌اندازی Areas (Admin, Creator)
- Layout پایه برای هر Area
- کانفیگ Serilog برای لاگینگ

---

## فاز ۱ — مدل‌ها و دیتابیس (۳ تا ۴ روز)

**هدف:** تمام جداول دیتابیس آماده شوند

- نوشتن تمام Domain Models
- نوشتن Enums
- تنظیم ApplicationDbContext
- نوشتن EF Configurations (Fluent API)
- ساخت Migration اولیه
- Seed کردن داده‌های پایه (دسته‌بندی‌ها، بسته‌های اعتباری)

---

## فاز ۲ — احراز هویت (۲ تا ۳ روز)

**هدف:** ثبت‌نام، ورود، و سطوح دسترسی کار کنند

- پیکربندی ASP.NET Identity
- صفحات Register، Login، ForgotPassword، ResetPassword
- تایید ایمیل
- سیاست‌های Authorization برای هر Area
- Middleware برای هدایت کاربران لاگین‌نشده

---

## فاز ۳ — مدیریت AI Providers (۳ تا ۴ روز)

**هدف:** ادمین بتواند مدل‌های AI را مدیریت کند

- IEncryptionService و AES-256
- ISettingService
- IAiProviderService
- صفحات Admin: مدیریت Providers و Models
- Seed داده‌های پیش‌فرض مدل‌ها
- تست اتصال به OpenRouter

---

## فاز ۴ — Creator Area — بخش ابزار (۵ تا ۷ روز)

**هدف:** سازنده بتواند ابزار بسازد

- IAppService و ISlugService
- صفحه ساخت ابزار (Wizard چند مرحله‌ای)
- Form Builder (صفحه Fields)
- رمزنگاری و ذخیره پرامپت
- آپلود نمونه خروجی (Showcase)
- ارسال برای بررسی
- صفحه لیست ابزارهای سازنده

---

## فاز ۵ — Admin Area — بررسی ابزار (۲ تا ۳ روز)

**هدف:** ادمین بتواند ابزارها را بررسی و تایید کند

- صفحه صف بررسی ابزارها
- مشاهده پرامپت رمزگشایی شده (فقط ادمین)
- تایید / رد با دلیل

---

## فاز ۶ — Execution Engine (۵ تا ۷ روز)

**هدف:** اجرای واقعی پرامپت کار کند

- IAiService — اتصال به OpenRouter
- IExecutionService — منطق کامل اجرا
- ICreditService — مدیریت اعتبار و درآمد
- پردازش داینامیک خروجی بر اساس OutputType
- IStorageService — ذخیره تصاویر و ویدیوها
- _OutputRenderer.cshtml — نمایش داینامیک خروجی
- مدیریت خطا و Refund اتوماتیک
- Rate Limiting

---

## فاز ۷ — Public Pages (۳ تا ۵ روز)

**هدف:** سایت عمومی آماده شود

- Layout زیبا و Responsive
- صفحه اصلی (Index)
- صفحه Explore با فیلتر
- صفحه جزئیات ابزار با فرم داینامیک
- پروفایل عمومی سازنده
- _AppCard و سایر Partials

---

## فاز ۸ — پرداخت ZarinPal (۳ تا ۴ روز)

**هدف:** کاربران بتوانند اعتبار بخرند

- IPaymentService
- صفحه تنظیمات ZarinPal در Admin
- صفحات خرید اعتبار
- جریان کامل ZarinPal (Request → Callback → Verify)
- Webhook handling
- صفحات موفقیت و شکست

---

## فاز ۹ — User Area (۲ تا ۳ روز)

**هدف:** داشبورد کاربر کامل شود

- تاریخچه اجراها
- کیف‌پول و تاریخچه تراکنش‌ها
- ابزارهای مورد علاقه
- صفحه BecomeCreator

---

## فاز ۱۰ — Creator Analytics و Earnings (۲ تا ۳ روز)

**هدف:** سازنده درآمدش را ببیند و برداشت کند

- داشبورد سازنده با نمودارها
- صفحه آنالیتیکس هر ابزار
- صفحه مدیریت درآمد
- جریان درخواست برداشت
- مدیریت درخواست برداشت در Admin

---

## فاز ۱۱ — Admin کامل (۳ تا ۴ روز)

**هدف:** تمام امکانات Admin تکمیل شود

- مدیریت کاربران
- مدیریت تمام پرداخت‌ها
- گزارش‌ها
- تنظیمات عمومی
- AdminAuditLog

---

## فاز ۱۲ — ویژگی‌های اجتماعی (۲ تا ۳ روز)

**هدف:** سیستم نظر، امتیاز، و اشتراک‌گذاری

- IReviewService
- نظر و امتیاز در صفحه ابزار
- عمومی کردن خروجی توسط کاربر
- Founding Creator Badge

---

## فاز ۱۳ — بهینه‌سازی و تجربه کاربری

- Caching برای صفحات پرترافیک
- بهینه‌سازی سئو (Meta tags، Canonical، Sitemap)
- پیام‌های Toast اطلاع‌رسانی
- Loading states حین اجرا
- Pagination و Infinite Scroll
- اطمینان از Responsive بودن در موبایل

---

## ترتیب پیشنهادی کار

```
فاز ۰ → ۱ → ۲ → ۳ → ۴ → ۵ → ۶ → ۷ → ۸ → ۹ → ۱۰ → ۱۱ → ۱۲ → ۱۳
```

منطق این ترتیب:
- بدون مدل‌ها و Auth هیچ‌چیز ممکن نیست
- بدون AI Providers، Creator نمی‌تواند ابزار بسازد
- بدون ابزار، Execution Engine چیزی برای اجرا ندارد
- بدون Execution، Payment بی‌معناست
