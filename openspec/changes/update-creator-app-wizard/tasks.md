## 1. Backend

- [x] 1.1 اضافه کردن `FieldsJson` به `CreateForm` و `EditForm`
- [x] 1.2 اضافه کردن `FieldDto` inner class در Create و Edit page models
- [x] 1.3 ذخیره `AppInputField` ها از `FieldsJson` پس از ساخت app (Create.cshtml.cs)
- [x] 1.4 sync فیلدها در ویرایش: RemoveRange + re-insert از FieldsJson (Edit.cshtml.cs)
- [x] 1.5 اضافه کردن `OutputType?` و `AiModelId?` به `UpdateAppDto`
- [x] 1.6 بروزرسانی `AppService.UpdateAppAsync` برای مدیریت OutputType و AiModelId
- [x] 1.7 Redirect پس از Create به `/Apps/Index`
- [x] 1.8 رفع bug: hidden input برای AiModelId و OutputType هنگام CanEditPrompt=false
- [x] 1.9 رفع bug: try/catch برای JsonException در deserialization FieldsJson

## 2. Frontend — Create

- [x] 2.1 بازنویسی مرحله ۳: دو ستون (field builder چپ + prompt editor راست)
- [x] 2.2 فرم افزودن فیلد با validation (name regex, uniqueness)
- [x] 2.3 نمایش فیلدها با دکمه‌های جابجایی و حذف
- [x] 2.4 `serializeFields()` روی submit فرم
- [x] 2.5 Quick-picker bar با تشخیص `{` در textarea
- [x] 2.6 `insertField(name)` برای درج `{name}` در مکان cursor
- [x] 2.7 Variable chips (نارنجی = defined, قرمز = undefined)
- [x] 2.8 Popovers راهنما روی عناوین بخش‌ها
- [x] 2.9 `filterModels(outputType)` برای فیلتر مدل‌ها بر اساس Capability

## 3. Frontend — Edit

- [x] 3.1 طراحی مجدد Edit.cshtml به ساختار ۳ مرحله‌ای مشابه Create
- [x] 3.2 ایجاد `Edit.js` با initialize از `editPageData.existingFields`
- [x] 3.3 نمایش راهنما برای System Context (readonly هنگام CanEditPrompt=false)
- [x] 3.4 نمایش بخش Prompt فقط هنگام CanEditPrompt=true
