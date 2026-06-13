# Spec: Error Logging System

## ADDED Requirements

### Requirement: Database Error Sink
The system MUST capture all Serilog log events at Warning level or above and persist them to the `ErrorLogs` database table without blocking the request thread.

#### Scenario: Warning logged during HTTP request
- Given a Warning or higher log event is emitted by any component
- When the `DatabaseSink.Emit()` is called
- Then the event is written to `ErrorLogChannel` (non-blocking `TryWrite`)
- And `ErrorLogWriterService` reads from the channel and saves to DB via a fresh DI scope
- And the HTTP request is not delayed

#### Scenario: No HTTP context (background/startup)
- Given a log event occurs outside an HTTP request
- When the sink reads `IHttpContextAccessor.HttpContext`
- Then it receives null and stores null for RequestPath, RequestMethod, UserId, UserName
- And the entry is still saved normally

#### Scenario: Channel full
- Given the bounded channel (capacity 2000) is full
- When a new log event arrives
- Then the oldest entry is dropped (`DropOldest`) and the new entry is written
- And the application never blocks or throws

---

### Requirement: Admin Error Log Page
Admins MUST be able to view, filter, analyze, resolve, and delete error log entries.

#### Scenario: View list with stats
- Given the admin navigates to `/Admin/ErrorLogs`
- Then they see 6 stat cards: Total, Critical, Error, Warning, Today, Resolved
- And a paginated list of 20 entries per page ordered by CreatedAt descending
- And each entry shows: level badge (color-coded), truncated message, request path, category, time, exception type, resolved/AI badges

#### Scenario: Filter by level and date
- Given the admin selects level "Error" and sets DateFrom
- When they submit the filter form
- Then only Error-level entries on or after DateFrom are shown
- And pagination resets to page 1

#### Scenario: AI analysis
- Given an active text AI model is configured
- When the admin clicks the AI button on a log entry
- Then a loading spinner appears on the button
- And after redirect the collapse for that entry auto-opens (via `OpenId` query param + JS)
- And the AI analysis is shown in a dark-themed box inside the collapse
- And the analysis is concise: علت (1 sentence), راه‌حل (≤2 sentences), اقدام فوری (1 sentence)

#### Scenario: No AI model configured
- Given no text AI model is active
- Then the AI button is disabled and a warning banner is shown

#### Scenario: Resolve entry
- Given the admin clicks the resolve (✓) button
- Then `IsResolved` toggles and a "حل‌شده" badge appears on the entry

#### Scenario: Delete and clear
- Given the admin clicks Delete on an entry with confirmation
- Then that single entry is removed
- Given the admin clicks "پاک‌کردن حل‌شده‌ها" with confirmation
- Then all resolved entries are removed
- Given the admin clicks "حذف همه" with confirmation
- Then all entries are deleted via a single `ExecuteDeleteAsync` SQL

---

### Requirement: ErrorLog Model
```
ErrorLog { Id(long), Level, Category, Message, ExceptionType?, StackTrace?,
           RequestPath?, RequestMethod?, UserId?, UserName?,
           CreatedAt, IsResolved(bool), AiAnalysis?, AiAnalyzedAt? }
```
