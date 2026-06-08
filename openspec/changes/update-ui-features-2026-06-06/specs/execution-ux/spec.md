## ADDED Requirements

### Requirement: Form Input Preservation on Failed Execution
When an execution fails and the detail page re-renders, all form field values SHALL be pre-populated from the previous submission.

#### Scenario: Text input preserved
- **WHEN** execution fails and page returns with error
- **THEN** all text, textarea, select, number, and checkbox fields retain their submitted values

### Requirement: Failed Execution Display
The `_OutputRenderer` partial SHALL display a styled error alert for failed executions instead of blank output.

#### Scenario: Failed execution shows error
- **WHEN** `Model.Status == ExecutionStatus.Failed`
- **THEN** an `alert-danger` with `Model.ErrorMessage` is shown and the renderer returns early

### Requirement: Re-run with Pre-filled Inputs
The re-run button on execution detail page SHALL navigate to the app page with inputs pre-filled from the previous execution.

#### Scenario: Failed execution re-run
- **WHEN** user clicks "اجرای مجدد" on a failed execution detail page
- **THEN** the app form loads with all previous input values populated from `LastExecution.InputValues`

### Requirement: Dashboard Shows Only Completed Executions
The user dashboard "آخرین اجراها" section SHALL only display `ExecutionStatus.Completed` executions.

#### Scenario: Failed executions hidden from dashboard
- **WHEN** user views dashboard
- **THEN** only the 5 most recent successful executions appear; failed executions are accessible via "تاریخچه اجراها"

### Requirement: Execution History All Rows Clickable
Every row in the execution history table SHALL navigate to the execution detail page, regardless of status.

#### Scenario: Click on failed execution row
- **WHEN** user clicks any row in `/User/Executions`
- **THEN** navigates to `/user/executions/{id}` for that execution

### Requirement: Logout Button in All Layouts
Creator and Admin layouts SHALL display a logout button in the sidebar footer.

#### Scenario: Logout visible in dark layouts
- **WHEN** user is in Creator or Admin area
- **THEN** "خروج از حساب" button appears below "بازگشت به سایت" link in red color

### Requirement: Test Payment Bypass
When payment gateway is not configured, selecting a credit package SHALL directly add credits without going through ZarinPal.

#### Scenario: Direct credit add
- **WHEN** user selects a package and gateway is not configured
- **THEN** credits are added via `ICreditService.AddCreditsAsync` and user is redirected to Success page

### Requirement: Detailed AI Error Messages
When the AI provider returns an error, the system SHALL extract and display the error message from the JSON response body.

#### Scenario: OpenRouter error displayed
- **WHEN** AI API returns non-2xx response
- **THEN** error message is extracted from `response.error.message` or `response.message` field
