# Public Pages

صفحاتی که بدون لاگین قابل دسترسی هستند.

---

## لیست صفحات

### Pages/Index.cshtml — صفحه اصلی
بخش‌های صفحه:
- Hero section: عنوان، توضیح کوتاه، دکمه شروع
- Featured Apps: ۸ ابزار برتر (بالاترین اجرا یا ادمین انتخاب می‌کند)
- Categories: نمایش دسته‌بندی‌ها با تعداد ابزار
- New Apps: جدیدترین ابزارهای اضافه شده
- Top Creators: برترین سازندگان
- Stats: تعداد ابزارها، اجراها، سازندگان

---

### Pages/Explore.cshtml — مرور ابزارها
- نمایش همه ابزارهای فعال با Grid layout
- Sidebar فیلتر:
  - دسته‌بندی (چندتایی)
  - نوع خروجی (Text / Image / Video / ...)
  - محدوده قیمت اعتباری
  - امتیاز (۴+، ۳+ و...)
- مرتب‌سازی: جدیدترین / پرکاربردترین / بالاترین امتیاز / کم‌هزینه‌ترین
- جستجو در عنوان و توضیحات
- صفحه‌بندی

---

### Pages/App/Detail.cshtml — صفحه ابزار
مسیر: `/app/{slug}`

بخش‌ها:
- تصویر شاخص، عنوان، توضیح، امتیاز، تعداد اجرا
- اطلاعات سازنده
- هزینه اعتباری
- **فرم اجرا** — فیلدها بر اساس AppInputField ها داینامیک ساخته می‌شوند
- **نمایش خروجی** — بر اساس OutputType داینامیک:
  - Text: Markdown renderer
  - Code: Syntax highlighted block
  - Image: img tag + دکمه دانلود
  - Video: video player
  - Audio: audio player
  - Form: فرم داینامیک از JSON schema
- گالری نمونه خروجی‌های سازنده (AppShowcaseItem)
- نظرات و امتیاز کاربران
- ابزارهای مشابه (همان دسته‌بندی)

نکته: دکمه اجرا فقط برای کاربران لاگین نمایش داده می‌شود. کاربران مهمان دعوت به ثبت‌نام می‌شوند.

---

### Pages/Category/Index.cshtml — صفحه دسته‌بندی
مسیر: `/category/{slug}`

- عنوان و توضیح دسته‌بندی
- ابزارهای این دسته با همان قابلیت فیلتر Explore

---

### Pages/Creator/Profile.cshtml — پروفایل عمومی سازنده
مسیر: `/creator/{username}`

- اطلاعات عمومی سازنده
- آمار: تعداد ابزار، مجموع اجراها، میانگین امتیاز
- لیست ابزارهای منتشر شده
- نشان Founding Creator در صورت وجود

---

### Pages/Creators/Index.cshtml — برترین سازندگان
- لیست سازندگان فعال مرتب بر اساس مجموع اجراها
- فیلتر بر اساس دسته‌بندی تخصص

---

### Pages/Credits/Packages.cshtml — خرید اعتبار
- نمایش بسته‌های فعال
- مقایسه بسته‌ها
- نمایش موجودی فعلی کاربر (اگر لاگین باشد)
- دکمه خرید → هدایت به ZarinPal

---

### Pages/Credits/Callback.cshtml — بازگشت از ZarinPal
- این صفحه فقط پردازش می‌کند (no UI)
- بعد از پردازش به Success یا Failed هدایت می‌کند

---

### Pages/Credits/Success.cshtml — پرداخت موفق
- پیام موفقیت
- موجودی جدید
- لینک به Explore

---

### Pages/Auth/Login.cshtml
- فرم ورود با Email و Password
- لینک "فراموشی رمز"
- لینک "ثبت‌نام"
- Remember Me

---

### Pages/Auth/Register.cshtml
- فرم ثبت‌نام
- انتخاب نقش: کاربر عادی یا سازنده
- تایید ایمیل الزامی است

---

### Pages/Auth/ForgotPassword.cshtml و ResetPassword.cshtml

---

### Partial Views مشترک

**_AppCard.cshtml** — کارت ابزار:
- تصویر شاخص
- عنوان و توضیح کوتاه
- نام سازنده
- امتیاز و تعداد اجرا
- هزینه اعتباری
- دکمه "مشاهده"

**_Navbar.cshtml**:
- لوگو
- لینک‌های اصلی: خانه / Explore / سازندگان
- سمت راست: موجودی اعتبار (اگر لاگین) + آواتار + dropdown

**_Footer.cshtml**

**_OutputRenderer.cshtml** — Partial برای نمایش خروجی داینامیک:
- یک OutputType و OutputData می‌گیرد
- بر اساس نوع، component مناسب را رندر می‌کند
