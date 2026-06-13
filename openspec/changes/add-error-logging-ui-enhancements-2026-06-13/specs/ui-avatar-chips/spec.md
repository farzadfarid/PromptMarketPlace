# Spec: User Avatar Chips

## ADDED Requirements

### Requirement: Avatar chip component
Every place that renders a user's DisplayName in the Admin or Creator panel MUST also render a 26×26px avatar circle.

#### Scenario: User has AvatarUrl
- Given a user has a non-empty `AvatarUrl`
- Then an `<img>` with `border-radius:50%` and `object-fit:cover` is shown beside the name

#### Scenario: User has no avatar
- Given a user's `AvatarUrl` is null or empty
- Then a grey circle with the first character of DisplayName in white is shown

#### Scenario: CSS classes
- `.user-chip` — flex container (gap .4rem, vertical-align middle)
- `.uc-avatar` — 26×26px circle, overflow hidden
- `.uc-initials` — grey background (#6c757d), white text, centered, bold

---

## MODIFIED Requirements

### Requirement: Admin pages show avatars
The following Admin pages MUST use the avatar chip pattern:
- Users/Index (requires `AvatarUrl` added to `UserRow` record)
- Apps/Index (creator column)
- Apps/Detail (creator info, executions list, reviews list)
- Apps/Review (creator header)
- Payments/Index
- Reviews/Index
- Withdrawals/Index (pending + processed)
- Reports/TopApps (requires `AvatarUrl` added to `CreatorRow` record)
- Reports/TopCreators

### Requirement: Creator pages show avatars
The following Creator pages MUST use the avatar chip pattern:
- Dashboard/Index (latest reviews — reviewer name)
- Reviews/Index (reviewer name)
- Apps/Executions (user column)
- Apps/ExecutionDetail (user stat card — larger 32px avatar above name)
