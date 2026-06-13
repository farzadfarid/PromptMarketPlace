## MODIFIED Requirements

### Requirement: IAiService / AiService
`AiService` به یک dispatcher ساده تبدیل شده که هیچ منطق API ندارد.

متدها:
- `RunAsync(AiModel model, string? apiKey, string? systemContext, string prompt, OutputType outputType, List<string>? inputImageUrls)` — بر اساس `model.Provider.ProviderType` استراتژی انتخاب و متد مناسب فراخوانی می‌شود
- `CheckVideoStatusAsync(string jobId, AiModel model, string? apiKey)` — به استراتژی provider واگذار می‌شود

وابستگی‌های constructor: `ProviderStrategyFactory`, `ILogger<AiService>`

#### Scenario: Dispatch بر اساس OutputType
- **WHEN** `RunAsync` با `OutputType.Text` فراخوانی شود
- **THEN** `strategy.RunChatAsync` فراخوانی شود

#### Scenario: Provider null
- **WHEN** `model.Provider` برابر null باشد
- **THEN** `AiResponse.Fail("مدل به سرویس‌دهنده متصل نیست.")` برگردانده شود

---

### Requirement: IStorageService / LocalStorageService
متد جدید `SaveBytesAsync` برای ذخیره داده‌های باینری:

```
Task<string> SaveBytesAsync(byte[] bytes, string folder, string extension)
```

- `bytes`: محتوای باینری
- `folder`: زیرپوشه در `uploads/` (مثلاً `"audio"`)
- `extension`: پسوند فایل (مثلاً `".mp3"`)
- بازگشت: مسیر relative (مثلاً `/uploads/audio/uuid.mp3`)

#### Scenario: ذخیره صدای باینری از AvalAI
- **WHEN** `AvalAiStrategy` پاسخ باینری audio دریافت کند
- **THEN** با `SaveBytesAsync(bytes, "audio", ".mp3")` فایل ذخیره شود
- **AND** مسیر local در `AiResponse.AudioUrl` برگردانده شود

---

## ADDED Requirements

### Requirement: IProviderStrategy Interface
interface ای که تمام استراتژی‌های AI provider باید پیاده‌سازی کنند:

```
RunChatAsync(model, apiKey, systemContext, prompt, inputImageUrls?) → AiResponse
RunImageAsync(model, apiKey, prompt, inputImageUrls?) → AiResponse
RunVideoAsync(model, apiKey, prompt, inputImageUrls?) → AiResponse
RunAudioAsync(model, apiKey, prompt) → AiResponse
CheckVideoStatusAsync(jobId, model, apiKey) → AiResponse
```

`BaseProviderStrategy` پیاده‌سازی پیش‌فرض `RunVideoAsync`, `RunAudioAsync`, `CheckVideoStatusAsync` را با پیام "پشتیبانی نمی‌شود" فراهم می‌کند.

#### Scenario: متد پیش‌فرض not-supported
- **WHEN** استراتژی‌ای `RunVideoAsync` را override نکرده باشد
- **THEN** `AiResponse.Fail("این سرویس‌دهنده از تولید ویدیو پشتیبانی نمی‌کند.")` برگردانده شود

---

### Requirement: ProviderStrategyFactory
سرویس `ProviderStrategyFactory` بر اساس `ProviderType` استراتژی مناسب را resolve می‌کند:
- `AvalAi` → `AvalAiStrategy`
- `Anthropic` → `AnthropicStrategy`
- `ChatQt` → `ChatQtStrategy`
- سایر موارد → `OpenAiCompatibleStrategy`

همه 4 استراتژی و Factory به‌صورت `Scoped` در DI ثبت می‌شوند.

#### Scenario: Resolve استراتژی
- **WHEN** `factory.Resolve(ProviderType.AvalAi)` فراخوانی شود
- **THEN** نمونه‌ای از `AvalAiStrategy` برگردانده شود
