# Decisions (v1)

## Core
- Single-owner timelines.
- Lanes (tracks) are core.
- Point-in-time and range items; open-ended ranges allowed (start required, end optional).
- Approx dates supported via `IsApprox` + `Precision` + partial components (Year/Month/Day).
- BCE + CE support is required.

## Visualization
- Two primary views: Horizontal Timeline + Vertical Feed with a switcher.
- Zoom levels: Day / Month / Year / Decade.
- Dense areas collapse/stack with “+N more”.

## Filtering
- Search covers: title + description + tags + lane names.
- Filters include: tags, lanes, types, date range, search text.

## Sharing & public
- Public viewing does not require login.
- Sharing is link-based.
- Public timelines can be unlisted or public+indexed (owner choice).

## Publish
- Draft vs Published flag on items; public views show only Published.

## Auth
- Passwordless OTP via Azure Communication Services:
  - Email OTP
  - WhatsApp OTP
- Identity provider: Entra External ID / Azure AD B2C.

## Data/Infra
- SQL Server local, Azure SQL in production.
- Media stored in Blob Storage.
- Single-tenant database with row-based ownership enforcement.

## Localization
- UI only (English + Spanish), selected per user.