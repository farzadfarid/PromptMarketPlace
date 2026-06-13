## 1. مدل و Enum

- [x] 1.1 ساخت `Models/Enums/ProviderType.cs` با مقادیر `OpenAiCompatible=0, AvalAi=1, Anthropic=2, ChatQt=3`
- [x] 1.2 افزودن `ProviderType ProviderType` به `Models/Domain/AiProvider.cs`

## 2. Strategy Layer

- [x] 2.1 ساخت `Services/Strategies/IProviderStrategy.cs` با 5 متد
- [x] 2.2 ساخت `Services/Strategies/BaseProviderStrategy.cs` با helpers مشترک و `RunOpenAiChatAsync` / `RunOpenAiImageAsync` قابل reuse
- [x] 2.3 ساخت `Services/Strategies/OpenAiCompatibleStrategy.cs`
- [x] 2.4 ساخت `Services/Strategies/AvalAiStrategy.cs` (ویدیو async، صدا باینری)
- [x] 2.5 ساخت `Services/Strategies/AnthropicStrategy.cs` (x-api-key، /messages)
- [x] 2.6 ساخت `Services/Strategies/ChatQtStrategy.cs` (images/videos array)
- [x] 2.7 ساخت `Services/Strategies/ProviderStrategyFactory.cs`

## 3. بروزرسانی سرویس‌ها

- [x] 3.1 بازنویسی `Services/AiService.cs` به dispatcher ساده
- [x] 3.2 افزودن `SaveBytesAsync` به `IStorageService`
- [x] 3.3 پیاده‌سازی `SaveBytesAsync` در `LocalStorageService`
- [x] 3.4 بروزرسانی signatures در `IAiProviderService` (افزودن `ProviderType` به Create/Update)
- [x] 3.5 بروزرسانی `AiProviderService.CreateProviderAsync` و `UpdateProviderAsync`
- [x] 3.6 بروزرسانی `ExecutionService.ProcessOutputAsync` برای مسیر local صدا

## 4. Admin UI

- [x] 4.1 افزودن `ProviderType` به `AiProviderFormViewModel`
- [x] 4.2 افزودن dropdown نوع سرویس‌دهنده به modal در `Providers.cshtml`
- [x] 4.3 نمایش badge نوع در جدول providers
- [x] 4.4 بروزرسانی `Providers.cshtml.cs.OnPostSaveAsync`
- [x] 4.5 بروزرسانی `openEdit` و `openCreate` در `Providers.js`

## 5. Program و Migration

- [x] 5.1 ثبت 4 استراتژی + Factory در `Program.cs`
- [x] 5.2 اجرای `dotnet ef migrations add AddProviderType`
- [x] 5.3 اجرای `dotnet ef database update`
