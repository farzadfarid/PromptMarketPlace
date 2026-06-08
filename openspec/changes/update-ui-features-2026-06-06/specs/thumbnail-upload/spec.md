## ADDED Requirements

### Requirement: App Thumbnail Upload in Create Form
The creator app creation form SHALL include an optional image upload field for the cover thumbnail.

#### Scenario: Upload and preview
- **WHEN** creator selects an image file (JPG/PNG/WEBP, max 2MB)
- **THEN** a live preview appears in the upload box before form submission

#### Scenario: File saved on submit
- **WHEN** the form is submitted with a thumbnail
- **THEN** the file is saved to `wwwroot/uploads/thumbnails/{guid}.ext` and `ThumbnailUrl` is stored in the database

#### Scenario: Upload box dimensions
- **WHEN** the upload box renders
- **THEN** it SHALL have `aspect-ratio: 750/404` and `max-width: 480px` matching the recommended image size

### Requirement: App Thumbnail Upload in Edit Form
The creator app edit form SHALL display the current thumbnail and allow replacing it.

#### Scenario: Existing thumbnail shown
- **WHEN** the edit page loads for an app with a thumbnail
- **THEN** the current image is pre-displayed in the upload box

#### Scenario: Thumbnail editable regardless of app status
- **WHEN** creator edits an Active or UnderReview app
- **THEN** thumbnail CAN be updated; only prompt editing is restricted to Draft/Suspended status

### Requirement: Thumbnail Size Guidance
Both create and edit forms SHALL display recommended image dimensions below the upload field.

#### Scenario: Guidance text visible
- **WHEN** upload field renders
- **THEN** text "اندازه توصیه‌شده: ۷۵۰ × ۴۰۴ پیکسل" is displayed as `form-text`
