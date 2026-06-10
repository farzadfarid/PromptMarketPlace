## MODIFIED Requirements

### Requirement: User Block Enforcement
سیستم MUST از ورود کاربر مسدود جلوگیری کند و تمام session‌های فعال او را حداکثر ظرف ۱ دقیقه باطل کند.

#### Scenario: ورود کاربر مسدود
- **WHEN** کاربری که `IsActive = false` دارد اطلاعات صحیح وارد می‌کند
- **THEN** بعد از `PasswordSignInAsync.Succeeded`، چک `IsActive` انجام می‌شود
- **AND** `SignOutAsync` فراخوانی می‌شود
- **AND** پیام «حساب کاربری شما مسدود شده است» نمایش داده می‌شود
- **AND** کاربر به صفحه login بازمی‌گردد

#### Scenario: باطل کردن session‌های فعال هنگام مسدودسازی
- **WHEN** ادمین کاربری را مسدود می‌کند (`IsActive = false`)
- **THEN** `UpdateSecurityStampAsync` فراخوانی می‌شود
- **AND** تمام cookie‌های فعال اون کاربر باطل می‌شوند
- **AND** حداکثر ۱ دقیقه بعد از مسدودسازی، کاربر از تمام session‌های باز خارج می‌شود

#### Scenario: بازه re-validation
- **WHEN** `SecurityStampValidatorOptions.ValidationInterval = 1 minute` تنظیم شده است
- **THEN** ASP.NET Identity هر ۱ دقیقه یکبار Security Stamp کاربر را با دیتابیس مقایسه می‌کند
- **AND** اگر مغایرت وجود داشته باشد، کاربر به صفحه login هدایت می‌شود
