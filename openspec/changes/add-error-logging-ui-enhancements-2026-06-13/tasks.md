# Tasks: add-error-logging-ui-enhancements-2026-06-13

All tasks completed on 2026-06-13.

## 1. Creator Execution Detail Page
- [x] Create `Areas/Creator/Pages/Apps/ExecutionDetail.cshtml.cs` with creator ownership check
- [x] Create `Areas/Creator/Pages/Apps/ExecutionDetail.cshtml` (mirrors Admin detail; adds user card with avatar)
- [x] Update `Areas/Creator/Pages/Apps/Executions.cshtml` — each row clickable, links to detail
- [x] Route: `/Creator/Apps/Executions/{appId}/{id}`

## 2. User Avatar Chips
- [x] Add `.user-chip`, `.uc-avatar`, `.uc-initials` CSS to `wwwroot/css/site.css`
- [x] Admin — Users/Index: add `AvatarUrl` to `UserRow` record and Select projection
- [x] Admin — Apps/Index, Apps/Detail (×3 places), Apps/Review
- [x] Admin — Payments/Index, Reviews/Index, Withdrawals/Index (×2 places)
- [x] Admin — Reports/TopApps, Reports/TopCreators (add `AvatarUrl` to `CreatorRow` record)
- [x] Creator — Dashboard/Index, Reviews/Index, Apps/Executions, Apps/ExecutionDetail

## 3. Error Logging System
- [x] `Models/Domain/ErrorLog.cs` — Level, Category, Message, ExceptionType, StackTrace, RequestPath, RequestMethod, UserId, UserName, CreatedAt, IsResolved, AiAnalysis, AiAnalyzedAt
- [x] `Infrastructure/Logging/ErrorLogChannel.cs` — bounded channel (2000 cap, DropOldest)
- [x] `Infrastructure/Logging/DatabaseSink.cs` — Serilog ILogEventSink, Warning+, writes to channel
- [x] `Infrastructure/Logging/ErrorLogWriterService.cs` — BackgroundService, reads channel, saves to DB
- [x] `Data/ApplicationDbContext.cs` — add `DbSet<ErrorLog> ErrorLogs`
- [x] `Program.cs` — two-stage Serilog init; `AddHttpContextAccessor`; `AddHostedService<ErrorLogWriterService>`
- [x] Migration `AddErrorLog` applied
- [x] `Areas/Admin/Pages/ErrorLogs/Index.cshtml.cs` — GET with stats+filters+paging(20/page); POST handlers: Analyze, Resolve, Delete, ClearResolved, ClearAll
- [x] `Areas/Admin/Pages/ErrorLogs/Index.cshtml` — stats bar, filter strip, card list with collapsible stack trace + AI box, pagination
- [x] Admin sidebar — new "پایش سیستم" section with "لاگ خطاها" nav item
- [x] After AI analysis redirect includes `OpenId` param; JS auto-opens and scrolls to that collapse

## 4. Input Sanitization & Media Rendering
- [x] `ExecutionService.cs` — filter `inputs` keys starting with `__` before saving `InputValues`
- [x] Admin `Executions/Detail.cshtml`, Creator `ExecutionDetail.cshtml`, User `Executions/Detail.cshtml` — filter `__` keys in foreach; fix `.Any()` guard
- [x] Admin `Executions/Detail.cshtml` + Creator `ExecutionDetail.cshtml` — detect audio (.mp3/.wav/.ogg/.m4a/.aac) and video (.mp4/.webm/.mov) file paths; render `<audio>`/`<video>` players with download button; `.TrimEnd('/')` on stored paths
