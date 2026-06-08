## ADDED Requirements

### Requirement: Modern App Card with Thumbnail
The `_AppCard.cshtml` partial SHALL display each app with a thumbnail image area, output-type badge, star rating, and credit cost using the `.app-card` CSS component.

#### Scenario: App with thumbnail image
- **WHEN** `App.ThumbnailUrl` is set
- **THEN** the image SHALL fill the 176px-tall card thumbnail area with `object-fit: cover` and zoom on hover

#### Scenario: App without thumbnail
- **WHEN** `App.ThumbnailUrl` is null or empty
- **THEN** a gradient placeholder with an output-type icon SHALL appear; each OutputType has a distinct orange gradient

#### Scenario: Output type badge
- **WHEN** the card renders
- **THEN** a semi-transparent dark badge with output type label SHALL appear at the bottom-right of the thumbnail

### Requirement: App Card Hover Effect
The `.app-card` component SHALL lift (`translateY(-5px)`) and show an orange border on hover.

#### Scenario: Hover state
- **WHEN** user hovers over an app card
- **THEN** card transforms upward 5px, border color changes to `--pm-primary`, and shadow deepens

### Requirement: Thumbnail in Creator App List
The creator's app list table SHALL show a 48×48px thumbnail (or gradient placeholder) next to each app title.

#### Scenario: Thumbnail visible in list
- **WHEN** creator views `/Creator/Apps`
- **THEN** each row shows a rounded 48×48 thumbnail or output-type gradient before the app title
