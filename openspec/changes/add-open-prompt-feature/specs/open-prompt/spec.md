## ADDED Requirements

### Requirement: درخواست پرامپت باز توسط سازنده
سازنده SHALL بتواند در صفحه ویرایش ابزار، درخواست نمایش عمومی پرامپت را ثبت یا لغو کند — فقط برای ابزارهای Active؛ ابزار تعلیق نمی‌شود.

#### Scenario: سازنده درخواست می‌دهد
- **WHEN** سازنده toggle «درخواست پرامپت باز» را فعال کند
- **THEN** `IsPromptPublicRequested` برای آن ابزار `true` می‌شود و پیام تایید نمایش داده می‌شود

#### Scenario: سازنده درخواست را لغو می‌کند
- **WHEN** سازنده toggle را غیرفعال کند
- **THEN** `IsPromptPublicRequested` به `false` برمی‌گردد و اگر قبلاً `IsPromptPublic` هم `true` بود، به `false` تبدیل می‌شود

#### Scenario: نمایش وضعیت در صفحه ویرایش
- **WHEN** سازنده صفحه Edit را باز کند
- **THEN** وضعیت فعلی نمایش داده می‌شود: «در انتظار تایید» (requested=true, public=false) | «تایید شده» (public=true) | «تایید نشده» (هر دو false)

### Requirement: تایید یا لغو پرامپت باز توسط ادمین
ادمین SHALL بتواند در صفحه جزئیات ابزار، درخواست سازنده را تایید یا لغو کند و همچنین بدون درخواست سازنده مستقیماً toggle کند.

#### Scenario: ادمین درخواست سازنده را تایید می‌کند
- **WHEN** `IsPromptPublicRequested = true` باشد و ادمین دکمه «تایید» را بزند
- **THEN** `IsPromptPublic` به `true` تبدیل می‌شود، AuditLog ثبت می‌شود، و اعلان برای creator ارسال می‌شود

#### Scenario: ادمین پرامپت باز را لغو می‌کند
- **WHEN** ادمین دکمه «لغو پرامپت باز» را بزند
- **THEN** هر دو `IsPromptPublic` و `IsPromptPublicRequested` به `false` برمی‌گردند و AuditLog ثبت می‌شود

#### Scenario: ادمین مستقیماً بدون درخواست سازنده فعال می‌کند
- **WHEN** `IsPromptPublicRequested = false` باشد و ادمین toggle را فعال کند
- **THEN** هر دو `IsPromptPublicRequested` و `IsPromptPublic` به `true` تبدیل می‌شوند

### Requirement: نمایش پرامپت در صفحه عمومی ابزار
سیستم SHALL پرامپت ابزار را فقط زمانی که `IsPromptPublic = true` است در صفحه عمومی نمایش دهد.

#### Scenario: نمایش بلاک پرامپت باز
- **WHEN** کاربر صفحه `/app/{slug}` را باز کند و `IsPromptPublic = true` باشد
- **THEN** بلاک «پرامپت باز» نمایش داده می‌شود شامل: متن پرامپت رمزگشایی‌شده، دکمه کپی با feedback آیکون، و توضیح «اجرای این ابزار X کردیت نیاز دارد»

#### Scenario: عدم نمایش وقتی تایید نشده
- **WHEN** `IsPromptPublic = false` باشد
- **THEN** هیچ بلاک یا اطلاعاتی از پرامپت نمایش داده نمی‌شود

#### Scenario: کپی پرامپت
- **WHEN** کاربر دکمه «کپی» را در بلاک پرامپت باز کلیک کند
- **THEN** متن پرامپت در clipboard کپی شود و آیکون دکمه برای ۲ ثانیه به تیک تبدیل شود

### Requirement: بدج «پرامپت باز» روی کارت ابزار
سیستم SHALL برچسب «پرامپت باز» را روی thumbnail کارت هر ابزار با `IsPromptPublic = true` نمایش دهد تا کاربران در لیست‌ها بتوانند آن را شناسایی کنند.

#### Scenario: نمایش بدج در کارت
- **WHEN** کارت ابزاری با `IsPromptPublic = true` رندر شود (صفحه Explore، صفحه اصلی، یا هر لیست دیگر)
- **THEN** بدج بنفش «پرامپت باز» با آیکون قفل‌باز (fa-lock-open) روی گوشه thumbnail کارت نمایش داده شود

#### Scenario: عدم نمایش بدج وقتی تایید نشده
- **WHEN** `IsPromptPublic = false` باشد
- **THEN** هیچ بدجی روی کارت نمایش داده نشود

### Requirement: فیلتر «فقط پرامپت باز» در صفحه Explore
صفحه Explore SHALL یک فیلتر برای نمایش فقط ابزارهای با پرامپت باز داشته باشد.

#### Scenario: فعال‌سازی فیلتر
- **WHEN** کاربر چک‌باکس «فقط پرامپت باز» را در sidebar فعال کند و فرم را submit کند
- **THEN** فقط ابزارهایی که `IsPromptPublic = true` دارند در نتایج نمایش داده شوند

#### Scenario: حفظ فیلتر در pagination
- **WHEN** فیلتر OnlyOpenPrompt فعال باشد و کاربر به صفحه بعدی برود
- **THEN** فیلتر در URL (`?OnlyOpenPrompt=true`) حفظ شود و نتایج همچنان فیلتر شده باقی بمانند

#### Scenario: حفظ فیلتر با تغییر sort
- **WHEN** فیلتر OnlyOpenPrompt فعال باشد و کاربر مرتب‌سازی را تغییر دهد
- **THEN** hidden input در فرم sort مقدار OnlyOpenPrompt را حفظ کند

#### Scenario: حفظ فیلتر در جستجو
- **WHEN** فیلتر OnlyOpenPrompt فعال باشد و کاربر جستجو کند
- **THEN** hidden input در search form مقدار OnlyOpenPrompt را حفظ کند
