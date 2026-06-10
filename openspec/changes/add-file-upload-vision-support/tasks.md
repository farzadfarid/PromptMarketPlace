## 1. Storage Layer
- [x] 1.1 افزودن `SaveUploadAsync(IFormFile, string folder)` به `IStorageService`
- [x] 1.2 پیاده‌سازی در `LocalStorageService`

## 2. AI Service
- [x] 2.1 افزودن `List<string>? inputImageUrls = null` به `IAiService.RunAsync`
- [x] 2.2 `RunChatCompletionAsync` — ساخت محتوای multimodal هنگام وجود تصویر (vision API)
- [x] 2.3 `RunVideoGenerationAsync` — ارسال آرایه `images` در request body

## 3. Execution Pipeline
- [x] 3.1 افزودن `List<string>? inputImageUrls = null` به `IExecutionService.ExecuteAsync`
- [x] 3.2 پاس دادن `inputImageUrls` به `_ai.RunAsync` در `ExecutionService`
- [x] 3.3 `InputValidator.Validate` — رد کردن MaxLength/MinLength برای فیلدهای FileUpload

## 4. Public Detail Page
- [x] 4.1 تغییر `<form>` به `enctype="multipart/form-data"`
- [x] 4.2 رندر فیلد `FileUpload` با drag-and-drop area و پیش‌نمایش تصویر
- [x] 4.3 `Detail.cshtml.cs` — آپلود فایل‌ها در `OnPostRunAsync`، ذخیره URL در Inputs، پاس دادن به ExecutionService

## 5. Creator Field Builder
- [x] 5.1 افزودن `FileUpload — آپلود تصویر` به dropdown انواع فیلد در Fields.cshtml
- [x] 5.2 پیش‌نمایش فیلد FileUpload در لایو پریویوی Fields.cshtml
