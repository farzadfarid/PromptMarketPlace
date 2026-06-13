## Context

پروژه با چند سرویس‌دهنده AI کار می‌کند که هر کدام پروتکل متفاوتی دارند:
- **AvalAI**: ویدیو async با polling، صدا باینری (نه JSON)، endpoint جداگانه `/videos`
- **Anthropic**: `/messages` به‌جای `/chat/completions`، `x-api-key` header به‌جای Bearer، فرمت response متفاوت
- **ChatQT**: ویدیو/تصویر از `chat/completions` با `message.images[]` / `message.videos[]`
- **OpenAI / OpenRouter**: استاندارد OpenAI-compatible

کد قبلی (`AiService.cs` تک‌فایل ~640 خط) همه این تفاوت‌ها را با fallback chain حدسی مدیریت می‌کرد.

## Goals / Non-Goals

- **Goals**: هر سرویس‌دهنده استراتژی مجزا داشته باشد؛ اضافه کردن provider جدید نیاز به تغییر منطق موجود نداشته باشد؛ ادمین نوع provider را در UI انتخاب کند
- **Non-Goals**: auto-detection نوع provider از BaseUrl؛ پشتیبانی همزمان از چند provider برای یک capability

## Decisions

- **Strategy Pattern با inheritance سطحی**: `BaseProviderStrategy` حاوی helpers مشترک و دو متد protected `RunOpenAiChatAsync` / `RunOpenAiImageAsync` است که توسط AvalAI و OpenAI-Compatible مستقیماً قابل استفاده‌اند. ChatQT و Anthropic این متدها را override می‌کنند.
- **`ILogger` (non-generic) در base**: تمام derived classes `ILogger<T>` خود را به constructor base پاس می‌دهند؛ چون `ILogger<T>` : `ILogger` است این بدون casting کار می‌کند.
- **`AvalAiStrategy` برای صدا**: response باینری audio را از طریق `IStorageService.SaveBytesAsync` ذخیره می‌کند و مسیر local برمی‌گرداند. `ExecutionService` اگر `AudioUrl` با `/uploads/` شروع شود، دانلود نمی‌کند.
- **ProviderType در DB**: integer با default=0 (`OpenAiCompatible`). Migration مقدار default را برای همه providerهای موجود تنظیم می‌کند.

## Alternatives Considered

- **Reflection / Plugin-based discovery**: پیچیده‌تر، برای تعداد محدود provider غیرضروری
- **Configuration-based (appsettings)**: خواناتر در config اما نیاز به restart برای تغییر provider type
- **Factory Method بدون Strategy**: کمتر extensible، منطق پراکنده‌تر

## Risks / Trade-offs

- **Migration ProviderType=0 برای همه**: providerهای AvalAI موجود تا زمان تغییر دستی ادمین با استراتژی OpenAI-Compatible کار می‌کنند (ویدیو و صدا ممکن است fail شوند). → ادمین باید پس از deploy نوع را در پنل به‌روز کند.
- **کد تکراری کم**: `RunOpenAiChatAsync` و `RunOpenAiImageAsync` در base class هستند و توسط OpenAI و AvalAI به اشتراک گذاشته می‌شوند؛ AnthropicStrategy و ChatQtStrategy کاملاً مستقل هستند.

## Migration Plan

1. Deploy کد جدید
2. `dotnet ef database update` (قبلاً اجرا شده)
3. ادمین: پنل → سرویس‌دهنده‌ها → ویرایش AvalAI → تنظیم نوع = **AvalAI**
4. تست اجرای ویدیو و صدا

## File Structure

```
Services/
├── AiService.cs                    ← dispatcher (50 خط)
├── Strategies/
│   ├── IProviderStrategy.cs        ← interface (5 متد)
│   ├── BaseProviderStrategy.cs     ← helpers + OpenAI shared methods
│   ├── OpenAiCompatibleStrategy.cs ← OpenAI, OpenRouter
│   ├── AvalAiStrategy.cs           ← AvalAI (video async, binary audio)
│   ├── AnthropicStrategy.cs        ← Anthropic Claude
│   ├── ChatQtStrategy.cs           ← ChatQT special parsing
│   └── ProviderStrategyFactory.cs  ← resolves by ProviderType
```
