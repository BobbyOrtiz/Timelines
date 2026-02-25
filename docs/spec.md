# Timelines v1 Spec

## Goal
Provide a mobile-first app to create, manage, and share timelines. Timelines support historical dates (BCE/CE), approximate dates, lanes/tracks, rich filtering, and media attachments.

## Personas
- Owner: creates and manages timelines and items.
- Viewer: accesses shared/public timeline without login.
- Collaborator (future-friendly): invited user with viewer/commenter/editor role (optional v1 depending on scope).
- Support/Admin: manages users and data through Admin app.

## Timeline
- Owned by a single user.
- Contains lanes (tracks).
- Contains items that belong to a lane (required in v1).
- Can be:
  - Private
  - Unlisted (anyone with link)
  - Public+Indexed (SEO discoverable)

## Timeline item
- Required fields: Lane, Type, Title, StartDate.
- Optional: Description, EndDate, Tags, Attachments.
- Dates:
  - BCE + CE supported.
  - Precision supported: Year / Month / Day (Time optional later).
  - Approx supported via boolean flag.
  - Range items: Start required; End optional (open-ended).
- Publish:
  - Items are Draft or Published.
  - Public views show Published only.
  - Owner can publish/unpublish items.

## Media attachments
- Images and videos.
- Stored in Blob Storage.
- Private by default; access via short-lived SAS from API.
- Thumbnails:
  - v1: image thumbnails supported (recommended); video thumbnails optional later.

## Views
- Horizontal Timeline view
  - Zoom Day/Month/Year/Decade
  - Collapsing stacks for dense regions, with expansion.
- Vertical Feed view
  - Chronological list, supports same filtering.
- Filtering
  - Search text across title/description/tags/lane name.
  - Filters by tags/lanes/type/date range.
  - Sorting by chronological start date; tie-breaker by configured item order.

## Import/Export
- Export:
  - Full timeline poster (PDF/PNG) auto-layout.
  - JSON export (backup/share).
- Import:
  - CSV minimal: Title, Date, Description.
  - JSON import (same schema as export).

## Localization
- UI localization: English, Spanish.
- Per-user language selection.

## Logging
- All projects use `ILogger`.
- Serilog configured per-app with consistent enrichers/correlation.

## Non-goals (v1)
- No moderation/reporting queue.
- No full version history of publishes (draft/published only).
- No offline mode.
- No calendar/3rd-party imports.
- No streaming transforms/HLS in v1.

## Success criteria
- User can create a timeline with lanes and items (including BCE dates).
- User can filter and view timeline in both visualizations.
- User can attach images/videos and view them reliably.
- User can share as unlisted or public+indexed; public view works without login.
- Publish toggles affect public visibility.