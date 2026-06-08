# قراردادهای فرانت‌اند

## کتابخانه‌های پایه

سه فایل زیر در `_Layout.cshtml` به صورت سراسری لود می‌شوند:

```html
<link href="~/css/vazirmatn.css" rel="stylesheet" />
<link href="~/css/bootstrap5-3-3.css" rel="stylesheet" />
<link href="~/css/font-awesome.css" rel="stylesheet" />
```

- **Vazirmatn**: فونت اصلی پروژه — تمام متون فارسی با این فونت نمایش داده می‌شوند
- **Bootstrap 5.3.3**: سیستم Grid، کامپوننت‌ها، و Utilities
- **Font Awesome**: آیکون‌ها در سراسر پروژه

این فایل‌ها local هستند و از CDN لود نمی‌شوند.

---

## قانون CSS و JavaScript اختصاصی هر صفحه

هر فایل `.cshtml` که نیاز به CSS یا JavaScript اختصاصی دارد، باید فایل‌های جداگانه با همان نام داشته باشد.

### نام‌گذاری

| فایل Razor | فایل CSS | فایل JavaScript |
|-----------|----------|----------------|
| `Index.cshtml` | `Index.css` | `Index.js` |
| `Explore.cshtml` | `Explore.css` | `Explore.js` |
| `Detail.cshtml` | `Detail.css` | `Detail.js` |
| `Fields.cshtml` | `Fields.css` | `Fields.js` |

### مکان فایل‌ها

فایل‌های CSS و JS هر صفحه در همان ساختار مسیر داخل `wwwroot` قرار می‌گیرند:

```
Pages/
├── Explore.cshtml
├── Explore.css          ← نه، اینجا نیست
...

wwwroot/
├── css/
│   ├── vazirmatn.css
│   ├── bootstrap5-3-3.css
│   ├── font-awesome.css
│   └── pages/
│       ├── public/
│       │   ├── Index.css
│       │   ├── Explore.css
│       │   └── app/
│       │       └── Detail.css
│       ├── creator/
│       │   ├── Dashboard.css
│       │   ├── apps/
│       │   │   ├── Create.css
│       │   │   └── Fields.css
│       │   └── earnings/
│       │       └── Index.css
│       ├── user/
│       │   └── ...
│       └── admin/
│           └── ...
└── js/
    └── pages/
        ├── public/
        │   ├── Index.js
        │   ├── Explore.js
        │   └── app/
        │       └── Detail.js
        ├── creator/
        │   ├── Dashboard.js
        │   └── apps/
        │       ├── Create.js
        │       └── Fields.js
        ├── user/
        │   └── ...
        └── admin/
            └── ...
```

### لود کردن در صفحه

هر صفحه CSS و JS خودش را از طریق Section لود می‌کند:

در `_Layout.cshtml`:
```html
@await RenderSectionAsync("Styles", required: false)
...
@await RenderSectionAsync("Scripts", required: false)
```

در هر `.cshtml`:
```html
@section Styles {
    <link rel="stylesheet" href="~/css/pages/public/Explore.css" />
}

@section Scripts {
    <script src="~/js/pages/public/Explore.js"></script>
}
```

---

## قوانین کلی

- **هیچ style inline** در فایل‌های cshtml نوشته نمی‌شود
- **هیچ script inline** در فایل‌های cshtml نوشته نمی‌شود — مگر برای پاس دادن داده از سرور به JS (متغیرهای init)
- اگر صفحه‌ای نیاز به CSS یا JS ندارد، فایل ساخته نمی‌شود
- CSS های مشترک بین چند صفحه در فایل‌های جداگانه با نام مناسب در `wwwroot/css/shared/` قرار می‌گیرند
- JS های مشترک (مثلاً toast notification) در `wwwroot/js/shared/` قرار می‌گیرند

---

## جهت صفحه (RTL)

چون پروژه فارسی است:

در `_Layout.cshtml`:
```html
<html lang="fa" dir="rtl">
```

Bootstrap 5 از RTL پشتیبانی می‌کند اما نسخه RTL آن باید استفاده شود یا override های لازم در یک فایل `rtl-fixes.css` نوشته شود.

---

## Partial Views

Partial هایی مثل `_AppCard.cshtml` و `_OutputRenderer.cshtml` CSS خودشان را دارند:

```
wwwroot/css/shared/
├── AppCard.css
├── OutputRenderer.css
└── Navbar.css
```

اما JS ندارند — منطق تعاملی آن‌ها در JS صفحه‌ای که Include می‌کند قرار می‌گیرد.
