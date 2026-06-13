# Spec: Input Sanitization & Media Rendering

## ADDED Requirements

### Requirement: Filter system fields from InputValues
The system MUST NOT store or display form fields whose names start with `__`.

#### Scenario: Anti-forgery token excluded at save time
- Given a form submission includes `__RequestVerificationToken`
- When `ExecutionService` iterates over inputs to create `InputValues`
- Then any key starting with `__` is skipped
- And the token is never written to the `ExecutionInputValues` table

#### Scenario: Legacy data filtered at display time
- Given older executions may already have `__` prefixed entries in the DB
- When any execution detail page renders `InputValues`
- Then the foreach iterates `InputValues.Where(i => !i.FieldName.StartsWith("__"))`
- And the `.Any()` guard uses the same filter
- Pages: Admin/Executions/Detail, Creator/Apps/ExecutionDetail, User/Executions/Detail

---

### Requirement: Media file inputs render as players
When an `InputValue.FieldValue` is a path under `/uploads/` pointing to a media file, it MUST render as an inline player, not as plain text.

#### Scenario: Audio input
- Given FieldValue matches `uploads/...` and extension is `.mp3|.wav|.ogg|.m4a|.aac`
- Then an `<audio controls class="w-100">` player is rendered
- And a download link is shown below

#### Scenario: Video input
- Given FieldValue matches `uploads/...` and extension is `.mp4|.webm|.mov`
- Then a `<video controls style="max-height:240px">` player is rendered
- And a download link is shown below

#### Scenario: Trailing slash in stored path
- Given a stored path ends with `/` (e.g. `uploads/inputs/file.mp3/`)
- When the page renders the value
- Then `.TrimEnd('/')` is applied before extension matching
- And the player `src` uses the trimmed path prefixed with `/`
