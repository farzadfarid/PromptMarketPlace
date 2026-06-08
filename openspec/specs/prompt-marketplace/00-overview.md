# AI App Marketplace — Project Overview

## تعریف محصول

یک مارکت‌پلیس که در آن سازندگان، پرامپت‌های هوش مصنوعی خود را به ابزارهای قابل استفاده (AI Apps) تبدیل می‌کنند. کاربر نهایی هرگز پرامپت اصلی را نمی‌بیند. سازنده فیلدهای ورودی و نوع خروجی را تعریف می‌کند. کاربر فرم را پر می‌کند و خروجی دریافت می‌کند.

## Stack فنی

- **Backend**: ASP.NET Core 9, C#
- **UI**: Razor Pages
- **ORM**: Entity Framework Core
- **Auth**: ASP.NET Identity
- **AI Gateway**: OpenRouter (اتصال یکجا به همه مدل‌ها)
- **Payment**: ZarinPal (کانفیگ از پنل ادمین)
- **Database**: SQL Server

## ساختار کلی پروژه

```
PromptMarketPlace/
├── Areas/
│   ├── Admin/Pages/
│   └── Creator/Pages/
├── Pages/                    ← Public
├── Models/
│   ├── Domain/
│   └── Enums/
├── ViewModels/
├── Services/
│   └── Interfaces/
├── Data/
│   ├── ApplicationDbContext.cs
│   └── Configurations/       ← EF Fluent API
├── Helpers/
│   ├── EncryptionHelper.cs
│   └── SlugHelper.cs
└── wwwroot/
```

## سه Area اصلی

| Area | دسترسی | توضیح |
|------|---------|-------|
| Public (Pages/) | همه | صفحه اصلی، مرور، اجرای ابزار |
| Creator (Areas/Creator) | نقش Creator | مدیریت ابزارها، آمار، درآمد |
| Admin (Areas/Admin) | نقش Admin | مدیریت کامل پلتفرم |

## فازها به ترتیب اولویت

1. زیرساخت و راه‌اندازی
2. مدل‌ها و دیتابیس
3. احراز هویت
4. مدیریت AI Providers
5. Public Pages
6. Creator Area
7. Execution Engine
8. سیستم اعتبار و ZarinPal
9. User Area
10. Admin Area
11. ویژگی‌های اجتماعی و بهینه‌سازی
