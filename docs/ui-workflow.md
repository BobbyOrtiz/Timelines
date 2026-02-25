# UI Workflow (MudBlazor)

## Global UI
- Mobile-first responsive layout.
- Top app bar with timeline selector and search/filter action.
- Language selector stored in user profile (en/es).

## Main screens
### 1) Landing / Browse
- If logged out:
  - Allow opening a public/unlisted timeline link.
  - Offer login (email OTP / WhatsApp OTP).
- If logged in:
  - Show user's timelines list (search, create).

### 2) Timeline view (toggle)
- Toggle: Timeline (horizontal) / Feed (vertical)
- Zoom control: Day / Month / Year / Decade
- Filter drawer/panel:
  - search text
  - lanes
  - tags
  - types
  - date range
  - drafts toggle (only for owner)
- Dense item areas:
  - collapse stack with “+N more” and expand on click/tap.

### 3) Item editor
- Fields: lane, type, title, description
- Dates:
  - Start: BCE/CE + precision + approx
  - End optional; allow “Ongoing” checkbox (End null)
- Tags selection/creation
- Media attachments:
  - upload images/videos
  - show thumbnails/gallery

### 4) Share & publish
- Publish action:
  - publish all drafts or selected items
- Sharing:
  - Create unlisted link
  - Toggle public + indexed
  - Copy link UI

## MudBlazor components (suggested)
- MudAppBar, MudDrawer, MudTabs, MudChipSet, MudSelect, MudDatePicker-like custom BCE picker
- MudTable for feed view and Admin lists
- Custom component for BCE date selection (Year/Month/Day + Era + approx + precision)