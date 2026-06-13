## MODIFIED Requirements

### Requirement: AiProvider Domain Model
موجودیت `AiProvider` نمایانگر یک سرویس‌دهنده هوش مصنوعی است.

فیلدهای موجود:
- `Name` — نام نمایشی (مثلاً "AvalAI")
- `BaseUrl` — آدرس پایه API (مثلاً `https://api.avalai.ir/v1`)
- `ApiKeyEncrypted` — API Key رمزنگاری شده با AES-256
- `IsActive` — فعال/غیرفعال کلی
- `Description` — توضیح اختیاری
- `CreatedAt`
- `BalanceUrl`, `BalanceJsonPath`, `BalanceCurrency` — پیکربندی بررسی موجودی
- `IsActiveForText`, `IsActiveForImage`, `IsActiveForVideo`, `IsActiveForAudio` — فعال بودن به‌ازای هر capability (حداکثر یکی فعال)

فیلد جدید:
- `ProviderType` — نوع سرویس‌دهنده (`int`, default=0)، مشخص می‌کند کدام استراتژی API باید برای این provider استفاده شود

#### Scenario: ایجاد provider با نوع مشخص
- **WHEN** ادمین یک provider جدید با `ProviderType = AvalAi` ایجاد کند
- **THEN** سیستم از `AvalAiStrategy` برای تمام فراخوانی‌های API آن provider استفاده کند

#### Scenario: مقدار پیش‌فرض ProviderType
- **WHEN** یک provider بدون تعیین `ProviderType` ایجاد شود
- **THEN** مقدار پیش‌فرض `0 = OpenAiCompatible` تنظیم شود

---

## ADDED Requirements

### Requirement: ProviderType Enum
سیستم باید از enum `ProviderType` با مقادیر زیر پشتیبانی کند:
- `OpenAiCompatible = 0` — پروتکل استاندارد OpenAI (OpenAI, OpenRouter, Gemini-compat و غیره)
- `AvalAi = 1` — AvalAI با endpoint `/videos` برای ویدیو و پاسخ باینری برای صدا
- `Anthropic = 2` — Anthropic Claude با `/messages` endpoint و `x-api-key` header
- `ChatQt = 3` — ChatQT که تصویر/ویدیو را از `chat/completions` با `message.images[]`/`message.videos[]` برمی‌گرداند

#### Scenario: تفکیک سرویس‌دهنده‌ها
- **WHEN** ادمین نوع provider را `AvalAi` تنظیم کند
- **THEN** همه درخواست‌های ویدیو به `/videos` (نه `/video/generations`) برود
- **AND** درخواست‌های صدا پاسخ باینری را مستقیم ذخیره کند

---

### Requirement: Strategy Pattern برای AI Providers
سیستم باید از الگوی Strategy برای ارسال درخواست به سرویس‌دهنده‌های مختلف AI استفاده کند.

Interface `IProviderStrategy` متدهای زیر را تعریف می‌کند:
- `RunChatAsync` — تولید متن/کد
- `RunImageAsync` — تولید تصویر
- `RunVideoAsync` — تولید ویدیو
- `RunAudioAsync` — تولید صدا
- `CheckVideoStatusAsync` — بررسی وضعیت job ویدیو

`ProviderStrategyFactory` بر اساس `ProviderType` استراتژی مناسب را برمی‌گرداند.

`AiService` فقط dispatcher است: OutputType را به متد مناسب استراتژی map می‌کند.

#### Scenario: Dispatcher AiService
- **WHEN** `AiService.RunAsync` با `OutputType.Video` فراخوانی شود
- **THEN** استراتژی متناسب با `model.Provider.ProviderType` انتخاب شود
- **AND** `strategy.RunVideoAsync` فراخوانی شود

#### Scenario: استراتژی پیش‌فرض
- **WHEN** `ProviderType` مقدار نامشخصی داشته باشد
- **THEN** `OpenAiCompatibleStrategy` استفاده شود

---

### Requirement: AvalAI Strategy
استراتژی AvalAI برای تفاوت‌های API این سرویس‌دهنده:

**ویدیو**:
- درخواست POST به `/videos` با پارامترهای `model`, `prompt`, `seconds="20"`
- اگر تصویر reference موجود باشد: multipart form با فیلد `input_reference`
- تصاویر local از disk خوانده می‌شوند (مسیر `/uploads/...`)
- پاسخ async: `id` (job ID) برگردانده می‌شود، polling هر 15 ثانیه تا `status=completed`
- پس از تکمیل: URL یا مسیر `/videos/{id}/content` برای دانلود

**صدا**:
- درخواست POST به `/audio/speech`
- پاسخ باینری (نه JSON) → مستقیماً با `IStorageService.SaveBytesAsync` ذخیره
- مسیر local (`/uploads/audio/uuid.mp3`) در `AiResponse.AudioUrl` برمی‌گردد

**متن و تصویر**:
- مثل `OpenAiCompatibleStrategy` (chat/completions و images/generations)

#### Scenario: ویدیو با تصویر reference
- **WHEN** درخواست ویدیو با `inputImageUrls` ارسال شود
- **THEN** سیستم تصویر را از disk بخواند و به‌عنوان multipart `input_reference` پیوست کند
- **AND** به endpoint `/videos` ارسال کند

#### Scenario: صدا با پاسخ باینری
- **WHEN** AvalAI پاسخ باینری audio برگرداند
- **THEN** سیستم bytes را مستقیم ذخیره کرده و مسیر local برگرداند
- **AND** `ExecutionService` دانلود مجدد نکند (چون مسیر با `/uploads/` شروع می‌شود)

---

### Requirement: Anthropic Strategy
استراتژی Anthropic برای Claude API:

- Endpoint: `POST /messages` (به‌جای `/chat/completions`)
- Auth: `x-api-key: {apiKey}` header (به‌جای Bearer token)
- Header اضافی: `anthropic-version: 2023-06-01`
- فرمت request: `{"model", "max_tokens", "system"?, "messages": [{"role", "content"}]}`
- فرمت response: `{"content": [{"type": "text", "text": "..."}], "usage": {"input_tokens", "output_tokens"}}`
- تولید تصویر، ویدیو و صدا پشتیبانی نمی‌شود (خطای واضح برمی‌گردد)

#### Scenario: درخواست chat به Anthropic
- **WHEN** `AnthropicStrategy.RunChatAsync` فراخوانی شود
- **THEN** درخواست به `/messages` با `x-api-key` header ارسال شود
- **AND** پاسخ از `content[0].text` استخراج شود

#### Scenario: تولید تصویر با Anthropic
- **WHEN** `AnthropicStrategy.RunImageAsync` فراخوانی شود
- **THEN** خطای `Anthropic از تولید تصویر پشتیبانی نمی‌کند` برگردانده شود

---

### Requirement: ChatQT Strategy
استراتژی ChatQT برای parsing خاص تصویر و ویدیو:

**تصویر**:
- ابتدا `images/generations` امتحان می‌شود
- در صورت 4xx (غیر از 401): `chat/completions` فراخوانی می‌شود
- parsing: `choices[0].message.images[0].image_url.url`

**ویدیو**:
- `chat/completions` با پرامپت ویدیو
- parsing: `choices[0].message.videos[0].video_url.url`
- fallback: URL mp4/webm در متن پاسخ

**صدا**: پشتیبانی نمی‌شود

#### Scenario: تولید تصویر از ChatQT
- **WHEN** `ChatQtStrategy.RunImageAsync` فراخوانی شود
- **AND** `/images/generations` با 404 یا 422 پاسخ دهد
- **THEN** سیستم `chat/completions` بزند و `message.images[0].image_url.url` را استخراج کند

#### Scenario: تولید ویدیو از ChatQT
- **WHEN** `ChatQtStrategy.RunVideoAsync` فراخوانی شود
- **THEN** درخواست به `chat/completions` برود
- **AND** URL ویدیو از `message.videos[0].video_url.url` خوانده شود

---

### Requirement: Admin UI مدیریت ProviderType
ادمین در صفحه مدیریت سرویس‌دهنده‌ها باید بتواند نوع هر provider را تعیین کند.

- Modal افزودن/ویرایش: dropdown با گزینه‌های OpenAI سازگار، AvalAI، Anthropic، ChatQT
- جدول providers: badge نوع هر provider در کنار نام نمایش داده می‌شود
- تغییر نوع بدون از دست دادن API Key یا سایر تنظیمات امکان‌پذیر است

#### Scenario: ویرایش نوع provider
- **WHEN** ادمین provider موجود را ویرایش کند
- **THEN** dropdown با مقدار فعلی `ProviderType` از پیش پر شده باشد
- **AND** پس از ذخیره، `ProviderType` در دیتابیس به‌روز شود
