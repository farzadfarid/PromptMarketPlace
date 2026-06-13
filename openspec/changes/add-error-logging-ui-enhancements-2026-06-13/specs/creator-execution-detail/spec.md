# Spec: Creator Execution Detail

## ADDED Requirements

### Requirement: Creator can view execution details
A creator MUST be able to view the full detail of any execution belonging to their own apps.

#### Scenario: Navigate from executions list
- Given the creator is on `/Creator/Apps/Executions/{appId}`
- When they click any row in the table
- Then they are navigated to `/Creator/Apps/Executions/{appId}/{id}`

#### Scenario: Ownership enforced
- Given a creator requests `/Creator/Apps/Executions/{appId}/{id}`
- When the app does not belong to their CreatorProfile
- Then a 404 is returned

#### Scenario: Detail page content
- Given the creator opens an execution detail
- Then they see 4 stat cards: User (with avatar), Date, Credits used, Tokens
- And user inputs are displayed (filtered, with image/audio/video rendering)
- And the output is rendered via `_OutputRenderer` partial

---

## MODIFIED Requirements

### Requirement: Executions list rows are clickable
Each row in the Creator executions list MUST link to the execution detail page.

#### Scenario: Click row
- Given the creator sees the executions table
- When they click any row
- Then `onclick="location.href='...'"` navigates to the detail page for that execution
