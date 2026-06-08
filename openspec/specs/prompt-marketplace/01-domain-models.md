# Domain Models

## بخش کاربران

### ApplicationUser
فیلدهای اضافه نسبت به IdentityUser:
- DisplayName
- AvatarUrl
- Role (enum: User / Creator / Admin)
- CreatedAt
- IsActive

روابط: یک Wallet، یک CreatorProfile (اختیاری)، چند AppExecution

---

### CreatorProfile
- UserId (FK)
- Bio
- WebsiteUrl
- IsVerified
- IsFoundingCreator — نشان ویژه برای اولین ۱۰۰ سازنده
- CommissionPercent — پیش‌فرض ۷۰، قابل تنظیم توسط ادمین
- JoinedAt

---

### UserWallet
- UserId (FK)
- CreditBalance — اعتبار قابل استفاده
- EarningBalance — درآمد انباشته سازنده (قابل برداشت)
- TotalEarned — کل درآمد تاریخی
- TotalWithdrawn — کل برداشت‌ها

---

### WalletTransaction
- UserId (FK)
- Type (enum: Purchase / Spend / Earn / Withdraw / Refund / AdminAdjust)
- CreditAmount (nullable)
- MoneyAmount (nullable) — ریال
- Description
- ReferenceId — شناسه پرداخت یا اجرا
- CreatedAt

---

## بخش ابزارها (Apps)

### AiApp
- Slug — یکتا، URL-friendly: `marketing-copy-generator`
- Title
- ShortDescription — حداکثر ۱۶۰ کاراکتر
- Description — متن کامل (Markdown)
- ThumbnailUrl
- Status (enum: Draft / UnderReview / Active / Suspended)
- CreditCost — هزینه هر اجرا بر حسب اعتبار
- OutputType (enum: Text / Image / Video / Form / Code / Audio)
- AiProviderId (FK) — کدام مدل AI اجرا کند
- EncryptedPrompt — پرامپت اصلی رمزنگاری شده با AES-256
- SystemContext — دستور سیستمی (رمزنگاری نشده)
- CategoryId (FK)
- CreatorId (FK → CreatorProfile)
- ExecutionCount
- AverageRating
- CreatedAt / UpdatedAt

روابط: چند AppInputField، چند AppExecution، چند AppReview، چند AppShowcaseItem، چند AppTag

---

### AppInputField
هر ابزار می‌تواند چند فیلد ورودی داشته باشد که فرم کاربر را می‌سازند.

- AppId (FK)
- Name — نام متغیر داخل پرامپت: `brand_name` → `{brand_name}`
- Label — عنوان نمایشی: "نام برند"
- Placeholder
- HelpText — راهنمایی زیر فیلد
- Type (enum: Text / Textarea / Select / Number / Checkbox / FileUpload / DatePicker)
- Options — JSON برای Select: `[{"value":"fa","label":"فارسی"},...]`
- IsRequired
- MinLength / MaxLength
- SortOrder — ترتیب نمایش

---

### AppCategory
- Name
- Slug
- IconClass — مثلاً `bi-image` برای Bootstrap Icons
- Description
- SortOrder

---

### AppTag
- AppId (FK)
- TagName

---

## بخش اجرا (Execution)

### AppExecution
- AppId (FK)
- UserId (FK)
- Status (enum: Pending / Running / Completed / Failed / Refunded)
- OutputType — کپی از AiApp.OutputType در زمان اجرا
- OutputText (nullable)
- OutputImageUrl (nullable)
- OutputVideoUrl (nullable)
- OutputFormSchema (nullable) — JSON schema برای خروجی نوع Form
- CreditUsed
- TokensUsed (nullable)
- ActualApiCost — هزینه واقعی API به دلار (برای آمار ادمین)
- ErrorMessage (nullable)
- Duration — مدت زمان اجرا
- IsPublic — آیا کاربر خروجی را عمومی کرده
- CreatedAt

---

### ExecutionInputValue
مقادیر ورودی هر اجرا برای آمار و نمایش تاریخچه (بدون پرامپت)

- ExecutionId (FK)
- FieldName
- FieldValue

---

### AppShowcaseItem
نمونه خروجی‌هایی که سازنده آپلود می‌کند برای نمایش عمومی

- AppId (FK)
- OutputType
- OutputUrl / OutputText
- Caption
- SortOrder
- CreatedAt

---

## بخش مالی

### CreditPackage
- Name — مثلاً "بسته استارتر"
- CreditAmount — مقدار اعتبار
- PriceRial — قیمت به ریال
- IsActive
- IsBestValue — برچسب "بهترین انتخاب"
- SortOrder

---

### Payment
- UserId (FK)
- PackageId (FK)
- Amount — مبلغ به ریال
- CreditAmount — اعتبار خریداری شده
- Status (enum: Pending / Verified / Failed / Refunded)
- ZarinpalAuthority — شناسه موقت ZarinPal
- ZarinpalRefId — شناسه نهایی پس از تایید
- CreatedAt / VerifiedAt

---

### WithdrawalRequest
درخواست برداشت درآمد از طرف سازنده

- CreatorId (FK)
- Amount — مبلغ درخواستی به ریال
- Status (enum: Pending / Approved / Rejected / Paid)
- BankAccountInfo — JSON: شماره شبا، نام صاحب حساب
- AdminNote (nullable)
- CreatedAt / ProcessedAt

---

## بخش اجتماعی

### AppReview
- AppId (FK)
- UserId (FK)
- Rating — ۱ تا ۵
- Comment (nullable)
- IsVerifiedPurchase — آیا حداقل یک بار اجرا کرده
- CreatedAt

---

## بخش تنظیمات سیستم

### SystemSetting
- Key (unique)
- Value
- Group — مثلاً: "ZarinPal" / "AI" / "Commission" / "General"
- Description
- IsEncrypted — مقادیر حساس مثل API Key رمزنگاری شوند

---

## Enums کامل

```
UserRole:         User / Creator / Admin
AppStatus:        Draft / UnderReview / Active / Suspended
OutputType:       Text / Image / Video / Form / Code / Audio
FieldType:        Text / Textarea / Select / Number / Checkbox / FileUpload / DatePicker
ExecutionStatus:  Pending / Running / Completed / Failed / Refunded
TransactionType:  Purchase / Spend / Earn / Withdraw / Refund / AdminAdjust
PaymentStatus:    Pending / Verified / Failed / Refunded
WithdrawalStatus: Pending / Approved / Rejected / Paid
AiCapability:     TextGeneration / ImageGeneration / VideoGeneration / CodeGeneration / AudioGeneration
```
