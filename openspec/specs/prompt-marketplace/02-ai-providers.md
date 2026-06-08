# AI Provider Management

## مفهوم کلی

ادمین از پنل خود می‌تواند مدل‌های هوش مصنوعی را مدیریت کند. هر مدل یک یا چند نوع خروجی را پشتیبانی می‌کند. صاحب ابزار وقتی نوع خروجی را انتخاب می‌کند، فقط مدل‌های سازگار نمایش داده می‌شوند.

## مدل‌های دیتابیس

### AiProvider
نمایانگر یک گیت‌وی یا سرویس‌دهنده است (نه مدل):

- Name — مثلاً "OpenRouter"
- BaseUrl
- ApiKeyEncrypted — رمزنگاری شده در دیتابیس
- IsActive
- Description
- CreatedAt

در نسخه اول احتمالاً فقط OpenRouter ثبت می‌شود چون اکثر مدل‌ها را پوشش می‌دهد.

---

### AiModel
هر مدل تعریف شده توسط ادمین:

- AiProviderId (FK)
- Name — نام نمایشی: "Claude Sonnet 4.6"
- ModelId — شناسه API: `anthropic/claude-sonnet-4-6`
- Description
- Capabilities — JSON array از AiCapability enum
  - مثال: `["TextGeneration", "CodeGeneration"]`
  - مثال: `["ImageGeneration"]`
  - مثال: `["VideoGeneration"]`
- CostPer1KTokens (nullable) — برای مدل‌های متنی
- CostPerImage (nullable) — برای مدل‌های تصویری
- CostPerSecondVideo (nullable) — برای مدل‌های ویدیویی
- MaxTokens (nullable)
- IsActive
- IsDefault — آیا مدل پیش‌فرض برای نوع خروجی مربوطه است
- SortOrder

---

## نمونه داده‌های پیش‌فرض

### مدل‌های متنی (TextGeneration / CodeGeneration)
| Name | ModelId |
|------|---------|
| Claude Sonnet 4.6 | anthropic/claude-sonnet-4-6 |
| Claude Haiku 4.5 | anthropic/claude-haiku-4-5 |
| GPT-4o | openai/gpt-4o |
| GPT-4o Mini | openai/gpt-4o-mini |
| Gemini 2.5 Pro | google/gemini-2.5-pro |

### مدل‌های تصویری (ImageGeneration)
| Name | ModelId |
|------|---------|
| FLUX 1.1 Pro | black-forest-labs/flux-1.1-pro |
| FLUX Schnell | black-forest-labs/flux-schnell |
| Stable Diffusion 3.5 | stabilityai/stable-diffusion-3-5 |
| DALL-E 3 | openai/dall-e-3 |

### مدل‌های ویدیویی (VideoGeneration)
| Name | ModelId |
|------|---------|
| Kling 1.6 Pro | kling/kling-1-6-pro |
| Runway Gen-4 | runway/gen-4 |
| Luma Dream Machine | luma/dream-machine |

---

## جریان کاری برای صاحب ابزار

1. صاحب ابزار نوع خروجی را انتخاب می‌کند (Text / Image / Video / ...)
2. سیستم تمام مدل‌های فعال با آن Capability را از دیتابیس می‌گیرد
3. صاحب ابزار یکی را انتخاب می‌کند
4. AiApp.AiModelId ذخیره می‌شود

---

## تنظیمات در Admin Area

### صفحه مدیریت Providers
- مشاهده لیست providers
- افزودن / ویرایش provider
- تنظیم API Key (ذخیره رمزنگاری شده)
- فعال/غیرفعال کردن

### صفحه مدیریت Models
- مشاهده تمام مدل‌ها (فیلتر بر اساس provider و capability)
- افزودن مدل جدید
- تنظیم هزینه
- تعیین مدل پیش‌فرض برای هر نوع خروجی
- فعال/غیرفعال کردن

---

## نحوه استفاده در Execution Engine

هنگام اجرا:
1. AiApp.AiModelId خوانده می‌شود
2. مدل و provider مرتبط از دیتابیس خوانده می‌شود
3. API Key از provider گرفته و رمزگشایی می‌شود
4. درخواست به BaseUrl + ModelId ارسال می‌شود
5. هزینه واقعی API در AppExecution.ActualApiCost ذخیره می‌شود

چون همه از OpenRouter می‌روند، ساختار Request یکسان است و فقط ModelId تغییر می‌کند.

---

## نکته مهم: محاسبه هزینه و اعتبار

ادمین باید CreditCost پیشنهادی را بر اساس هزینه واقعی تنظیم کند:

- هزینه API برای یک اجرا: X دلار
- تبدیل به ریال: X × نرخ دلار
- ضریب سود: مثلاً ۳×
- تبدیل به اعتبار: بر اساس نرخ ریال به اعتبار

این محاسبه باید در صفحه ساخت ابزار به صاحب ابزار نمایش داده شود تا قیمت‌گذاری معقول انجام دهد.
