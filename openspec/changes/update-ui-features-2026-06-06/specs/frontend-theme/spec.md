## ADDED Requirements

### Requirement: Orange Brand Color System
The system SHALL use orange (#f97316) as the sole primary brand color across all UI elements by overriding Bootstrap CSS variables at the `:root` level in `site.css`.

#### Scenario: Bootstrap utility classes render orange
- **WHEN** any Bootstrap utility class referencing primary color is used (e.g., `bg-primary`, `text-primary`, `btn-primary`)
- **THEN** the rendered color SHALL be orange (#f97316) not Bootstrap default blue

#### Scenario: Success and info colors are orange-toned
- **WHEN** `text-success`, `bg-success`, `btn-success`, `text-info`, `bg-info` are used
- **THEN** they SHALL render in orange variants, not green or cyan

### Requirement: Dark Layout Table Text Visibility
All three dark layouts (admin, creator, user) SHALL override `--bs-table-color` so table cell text is visible on dark backgrounds.

#### Scenario: Table cells readable in dark theme
- **WHEN** a table is rendered inside `.admin-layout`, `.creator-layout`, or `.user-layout`
- **THEN** all `td` and `th` elements SHALL have `color: #e2e8f0 !important`

### Requirement: Form Help Text Visibility in Dark Theme
The `.form-text` CSS class SHALL be visible in all dark layouts.

#### Scenario: Help text readable
- **WHEN** a `<div class="form-text">` appears inside a dark layout
- **THEN** text color SHALL be `#64748b` (slate-500), not the default dark browser color
