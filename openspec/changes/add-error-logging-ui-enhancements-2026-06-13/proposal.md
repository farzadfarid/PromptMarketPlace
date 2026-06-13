# Proposal: Error Logging, UI Enhancements & Creator Execution Detail

**Change ID:** add-error-logging-ui-enhancements-2026-06-13  
**Date:** 2026-06-13  
**Status:** Implemented

## Summary

Four distinct improvements implemented in one session:

1. **Creator Execution Detail** — Creators can now view full execution details (inputs, outputs, user info) for their own apps, mirroring the existing Admin detail page.
2. **User Avatar Chips** — Every place that shows a user's display name in Admin and Creator panels now also shows the user's avatar (or an initials fallback circle).
3. **Error Logging System** — A Serilog database sink captures Warning+ level logs into `ErrorLogs` table; an admin page provides full list/filter/AI-analysis/resolve/delete UX.
4. **Input Sanitization & Media Rendering** — `__RequestVerificationToken` is filtered out at both save time and display time; audio/video inputs in `InputValues` now render as `<audio>`/`<video>` players instead of raw text.

## Motivation

- Creators had no visibility into how their apps were being used (inputs, outputs, errors).
- Admin and Creator panels showed only names — no visual identity cues, making lists hard to scan.
- Errors were written to rolling log files only; no searchable UI, no AI-assisted diagnosis.
- Anti-forgery tokens leaked into "User Inputs" section of execution detail pages.
- Uploaded audio/video files in inputs were displayed as raw file paths.

## Scope

| Area | Files Changed |
|------|--------------|
| Creator | New: `Apps/ExecutionDetail.cshtml(.cs)` |
| Admin + Creator | 14 `.cshtml` files updated for avatar chips; `Users/Index.cshtml.cs` updated |
| Error Logging | New: `Models/Domain/ErrorLog.cs`, `Infrastructure/Logging/` (3 files), `Areas/Admin/Pages/ErrorLogs/Index.cshtml(.cs)` |
| Program.cs | Two-stage Serilog, `AddHttpContextAccessor`, `AddHostedService<ErrorLogWriterService>` |
| DB | New migration: `AddErrorLog` |
| Execution Input | `ExecutionService.cs`, 3 detail pages |

## Non-Goals

- No email/Slack alerting on critical errors (future work).
- No log rotation/archival policy enforced in code.
- No real-time push notifications for new errors.
