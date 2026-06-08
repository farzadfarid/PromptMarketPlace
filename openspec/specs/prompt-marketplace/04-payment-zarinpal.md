# سیستم پرداخت — ZarinPal

## معماری کلی

- پرداخت فقط برای خرید اعتبار است
- سازندگان از طریق درخواست برداشت، درآمدشان را دریافت می‌کنند (پرداخت دستی توسط ادمین)
- تنظیمات ZarinPal از پنل ادمین انجام می‌شود، نه کد

---

## تنظیمات در Admin

در جدول SystemSetting با Group = "ZarinPal" ذخیره می‌شود:

| Key | توضیح |
|-----|-------|
| ZarinPal:MerchantId | شناسه پذیرنده زرین‌پال |
| ZarinPal:IsSandbox | true برای تست، false برای production |
| ZarinPal:Description | توضیح پیش‌فرض تراکنش |

ادمین در صفحه Settings > Payment این مقادیر را ویرایش می‌کند.
ZarinPal:MerchantId با IsEncrypted = true ذخیره می‌شود.

---

## جریان پرداخت

### مرحله ۱: انتخاب بسته
- کاربر به صفحه `/credits/packages` می‌رود
- بسته‌های فعال نمایش داده می‌شوند
- کاربر یکی را انتخاب می‌کند

### مرحله ۲: شروع پرداخت (Request)
- یک رکورد Payment با Status = Pending ساخته می‌شود
- درخواست به ZarinPal API ارسال می‌شود:
  - Amount: مبلغ به ریال
  - Description: "خرید X اعتبار"
  - CallbackUrl: `https://site.com/credits/callback`
  - Mobile / Email کاربر
- ZarinPal یک Authority برمی‌گرداند
- Authority در Payment.ZarinpalAuthority ذخیره می‌شود
- کاربر به صفحه پرداخت ZarinPal هدایت می‌شود

### مرحله ۳: Callback
پس از پرداخت، ZarinPal کاربر را به `/credits/callback` برمی‌گرداند با:
- Authority
- Status (OK یا NOK)

اگر Status = OK:
1. Payment پیدا می‌شود با Authority
2. درخواست Verify به ZarinPal ارسال می‌شود
3. ZarinPal یک RefId برمی‌گرداند
4. Payment.Status = Verified
5. Payment.ZarinpalRefId = RefId ذخیره می‌شود
6. اعتبار به کیف‌پول کاربر اضافه می‌شود
7. WalletTransaction با Type = Purchase ثبت می‌شود
8. کاربر به صفحه موفقیت هدایت می‌شود

اگر Status = NOK:
1. Payment.Status = Failed
2. کاربر به صفحه خطا هدایت می‌شود

### نکته امنیتی مهم
اعتبار فقط بعد از Verify موفق اضافه می‌شود، نه فقط بعد از Callback.
برای جلوگیری از اجرای دوباره، قبل از Verify بررسی شود Payment.Status != Verified.

---

## صفحات مرتبط

| صفحه | مسیر |
|------|------|
| لیست بسته‌ها | /credits/packages |
| صفحه پرداخت موفق | /credits/success |
| صفحه پرداخت ناموفق | /credits/failed |
| Callback ZarinPal | /credits/callback |
| تاریخچه خرید | /user/wallet/history |

---

## بسته‌های اعتباری

نمونه بسته‌ها (قابل تنظیم توسط ادمین):

| نام | اعتبار | قیمت (تومان) | توضیح |
|-----|--------|--------------|-------|
| آزمایشی | ۵۰ | ۲۵,۰۰۰ | برای اولین تجربه |
| استارتر | ۱۵۰ | ۶۰,۰۰۰ | ۲۰٪ تخفیف |
| حرفه‌ای | ۴۰۰ | ۱۴۰,۰۰۰ | ۳۰٪ تخفیف — بهترین انتخاب |
| سازمانی | ۱۰۰۰ | ۳۰۰,۰۰۰ | ۴۰٪ تخفیف |

---

## مدیریت در Admin Area

### صفحه مدیریت بسته‌ها
- افزودن / ویرایش / حذف بسته
- فعال/غیرفعال کردن
- تغییر ترتیب نمایش
- تعیین IsBestValue

### صفحه تراکنش‌ها
- جستجو بر اساس کاربر، تاریخ، وضعیت
- مشاهده جزئیات هر پرداخت
- امکان Refund دستی (اعتبار برگشت داده شود)

### صفحه مدیریت تنظیمات ZarinPal
- ویرایش MerchantId
- تغییر بین Sandbox و Production
- تست اتصال

---

## مدیریت درآمد سازندگان

سازنده از پنل Creator:
- موجودی درآمد را می‌بیند
- درخواست برداشت ثبت می‌کند
- شماره شبا وارد می‌کند

ادمین از پنل Admin:
- لیست درخواست‌های برداشت را می‌بیند
- پرداخت را دستی انجام می‌دهد
- وضعیت را Paid می‌کند
- کاربر اطلاع‌رسانی می‌شود
