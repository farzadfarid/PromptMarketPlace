## ADDED Requirements

### Requirement: File Upload Input Field
سازنده ابزار SHALL بتواند فیلد ورودی از نوع `FileUpload` تعریف کند که کاربر می‌تواند یک تصویر در آن آپلود کند.

#### Scenario: تعریف فیلد FileUpload توسط سازنده
- **WHEN** سازنده در صفحه Fields ابزار، نوع `FileUpload — آپلود تصویر` را انتخاب می‌کند
- **THEN** فیلد با نوع `FieldType.FileUpload` ذخیره می‌شود

#### Scenario: رندر فیلد FileUpload در فرم عمومی
- **WHEN** ابزار دارای فیلد FileUpload است و کاربر صفحه Detail را باز می‌کند
- **THEN** یک drag-and-drop area با دکمه انتخاب فایل نمایش داده می‌شود
- **AND** پس از انتخاب تصویر، پیش‌نمایش آن نمایش داده می‌شود
- **AND** فرم با `enctype="multipart/form-data"` ارسال می‌شود

#### Scenario: ذخیره فایل آپلود شده
- **WHEN** کاربر فرم ابزار را با فایل تصویر submit می‌کند
- **THEN** فایل در `wwwroot/uploads/inputs/` ذخیره می‌شود
- **AND** URL نسبی در `Inputs[fieldName]` برای substitution در prompt قرار می‌گیرد
- **AND** URL مطلق به لیست `inputImageUrls` اضافه می‌شود

#### Scenario: اعتبارسنجی FileUpload اجباری
- **WHEN** فیلد FileUpload با `IsRequired = true` تعریف شده و کاربر فایلی آپلود نکرده
- **THEN** خطای «فیلد الزامی است» نمایش داده می‌شود
- **AND** MaxLength و MinLength برای فیلدهای FileUpload اعمال نمی‌شوند
