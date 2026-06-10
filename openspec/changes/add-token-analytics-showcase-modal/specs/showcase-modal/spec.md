## ADDED Requirements

### Requirement: Showcase Items Clickable with Modal
هر showcase item در بخش "نمونه خروجی‌ها" در sidebar صفحه عمومی ابزار SHALL کلیک‌پذیر باشد و محتوای کامل را در یک Bootstrap Modal نمایش دهد.

داده آیتم از طریق HTML data attributes منتقل می‌شود:
- `data-caption`: عنوان آیتم (Razor auto-HTML-encode)
- `data-text`: متن خروجی
- `data-imgurl`: آدرس تصویر
- `data-type`: نوع خروجی (OutputType enum)

#### Scenario: کلیک روی آیتم متنی
- **WHEN** کاربر روی showcase item که `OutputText` دارد کلیک می‌کند
- **THEN** Modal با id `#showcaseModal` باز می‌شود
- **THEN** عنصر `<pre>` با `white-space:pre-wrap`, `font-family:inherit`, `font-size:.9rem`, `line-height:1.7` داخل modal body ساخته می‌شود
- **THEN** متن کامل بدون truncation نمایش داده می‌شود

#### Scenario: کلیک روی آیتم تصویری
- **WHEN** کاربر روی showcase item که `OutputUrl` دارد و `OutputType == Image` است کلیک می‌کند
- **THEN** Modal باز می‌شود
- **THEN** عنصر `<img>` با `class="img-fluid rounded"` و `src=imgUrl` داخل modal body ساخته می‌شود

#### Scenario: عنوان modal از Caption
- **WHEN** showcase item دارای Caption غیر خالی است
- **THEN** `showcaseModalTitle` با مقدار caption پر می‌شود
- **WHEN** Caption خالی است
- **THEN** عنوان پیش‌فرض "نمونه خروجی" استفاده می‌شود

---

### Requirement: Showcase Item Card Preview Truncation
هر showcase item در کارت sidebar SHALL محتوا را به ارتفاع محدود نمایش دهد و لینک "مشاهده کامل" داشته باشد.

#### Scenario: متن کوتاه‌شده در کارت
- **WHEN** showcase item در sidebar نمایش داده می‌شود
- **THEN** div متن با `max-height:56px; overflow:hidden` رندر می‌شود
- **THEN** لینک `<i class="fas fa-eye"> مشاهده کامل` زیر div متن نمایش داده می‌شود

#### Scenario: تصویر کاور در کارت
- **WHEN** showcase item از نوع Image است و OutputUrl دارد
- **THEN** `<img>` با `max-height:150px; object-fit:cover; width:100%` در کارت نمایش داده می‌شود

---

### Requirement: Showcase Modal JavaScript Handler
فایل `wwwroot/js/pages/public/Detail.js` SHALL click handler برای همه `.showcase-item` elements پیاده‌سازی کند.

#### Scenario: مقداردهی اولیه handler
- **WHEN** DOM آماده می‌شود (DOMContentLoaded)
- **THEN** `querySelectorAll('.showcase-item').forEach(...)` اجرا می‌شود
- **THEN** برای هر element یک click listener ثبت می‌شود

#### Scenario: خواندن data attributes در JS
- **WHEN** click handler اجرا می‌شود
- **THEN** مقادیر از `this.dataset.caption`, `this.dataset.text`, `this.dataset.imgurl` خوانده می‌شوند (HTML-decode خودکار توسط browser)
- **THEN** نه `JSON.parse` و نه `HTML.Raw` استفاده نمی‌شود
