## ADDED Requirements

### Requirement: Vision API Support
سرویس AI SHALL هنگامی که `inputImageUrls` ارائه می‌شود، پیام‌های multimodal (vision) را به API ارسال کند.

#### Scenario: ارسال پیام multimodal با تصویر
- **WHEN** `RunChatCompletionAsync` با `inputImageUrls` غیر خالی فراخوانی می‌شود
- **THEN** محتوای پیام کاربر به صورت آرایه‌ای از `{type:"text", text:...}` و `{type:"image_url", image_url:{url:...}}` ارسال می‌شود

#### Scenario: ارسال تصویر در تولید ویدیو
- **WHEN** `RunVideoGenerationAsync` با `inputImageUrls` فراخوانی می‌شود
- **THEN** آرایه `images` در request body به endpoint ویدیو ارسال می‌شود

### Requirement: Image URL Pipeline
سیستم SHALL آدرس‌های URL تصاویر آپلود شده را از صفحه Detail تا سرویس AI پاس دهد.

#### Scenario: پاس دادن image URLs در ExecutionService
- **WHEN** `ExecuteAsync` با `inputImageUrls` فراخوانی می‌شود
- **THEN** این لیست بدون تغییر به `IAiService.RunAsync` پاس داده می‌شود

#### Scenario: URL مطلق برای Vision API
- **WHEN** صفحه Detail فایل آپلود شده را ذخیره می‌کند
- **THEN** URL مطلق (`scheme://host/path`) به لیست `inputImageUrls` اضافه می‌شود تا API خارجی به آن دسترسی داشته باشد
