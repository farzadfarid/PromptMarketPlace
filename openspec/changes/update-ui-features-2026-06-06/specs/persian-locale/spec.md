## ADDED Requirements

### Requirement: Persian Number Formatting
All numeric values displayed to users SHALL use Eastern Arabic (Persian) digits via the `PersianHelper` extension methods.

#### Scenario: Execution count in Persian
- **WHEN** any count or monetary value is rendered in a CSHTML view
- **THEN** digits are displayed as ۰۱۲۳۴۵۶۷۸۹ not 0123456789

#### Scenario: N0Fa extension method
- **WHEN** `someInt.N0Fa()` or `someLong.N0Fa()` is called
- **THEN** returns a thousands-formatted Persian digit string (e.g., ۱۲,۳۴۵)

### Requirement: Shamsi (Solar Hijri) Date Formatting
All dates displayed to users SHALL be converted to the Persian (Shamsi) calendar using `PersianHelper.ToShamsi()`.

#### Scenario: Date converted to Shamsi
- **WHEN** `someDateTime.ToShamsi("yy/MM/dd")` is called
- **THEN** returns a Shamsi date string with Persian digits (e.g., ۰۵/۰۳/۱۵)

#### Scenario: Global helper availability
- **WHEN** any Razor view in Pages/ or Areas/ renders
- **THEN** `ToShamsi()`, `N0Fa()`, and `ToFarsiDigits()` are available via `@using PromptMarketPlace.Helpers` in all `_ViewImports.cshtml`

### Requirement: Persian Dropdown Options
All Select-type input fields in app execution forms SHALL display options in Persian or Persian (English) format.

#### Scenario: Art style options in Persian
- **WHEN** user views the image prompt generator execution form
- **THEN** art style options are shown as "واقع‌گرایانه", "هنر دیجیتال", etc.

#### Scenario: Programming language options bilingual
- **WHEN** user views the code generator execution form
- **THEN** language options show "پایتون (Python)", "جاوا اسکریپت (JavaScript)", etc.
