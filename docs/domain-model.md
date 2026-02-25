# Domain Model

## Entities (conceptual)

### UserProfile
- UserId (external identity subject)
- Email (optional, verified)
- Phone (optional, verified)
- PreferredLanguage (en/es)
- CreatedUtc, UpdatedUtc

### Timeline
- TimelineId
- OwnerUserId
- Title
- Description (optional)
- DefaultView (Timeline/Feed)
- DefaultZoom (Day/Month/Year/Decade)
- IsPublic (bool)
- IsIndexed (bool) // only meaningful if IsPublic
- CreatedUtc, UpdatedUtc

### Lane
- LaneId
- TimelineId
- Name
- SortOrder
- CreatedUtc, UpdatedUtc

### Tag
- TagId
- TimelineId
- Name
- CreatedUtc, UpdatedUtc

### TimelineItem
- ItemId
- TimelineId
- LaneId (required v1)
- Type (string or enum-like)
- Title
- Description (optional)
- StartDate (TimelineDate)
- EndDate (TimelineDate nullable)
- DisplayOrderTiebreaker (int)
- IsPublished (bool)
- PublishedUtc (nullable)
- CreatedUtc, UpdatedUtc

### ItemTag (join)
- ItemId
- TagId

### Attachment
- AttachmentId
- ItemId
- MediaType (Image/Video)
- BlobKey / BlobPath
- ContentType
- SizeBytes
- OriginalFileName
- Width, Height (optional)
- DurationSeconds (optional)
- ThumbnailBlobKey (optional)
- CreatedUtc

### ShareLink
- ShareLinkId
- TimelineId
- LinkToken (unguessable)
- AccessLevel (Viewer/Commenter/Editor) // recommended: allow Viewer/Commenter via link, Editor via invites
- IsEnabled
- ExpiresUtc (optional)
- CreatedUtc

### Collaborator (optional v1)
- TimelineId
- UserId
- Role (Viewer/Commenter/Editor)
- CreatedUtc

## TimelineDate (BCE/CE + precision + approximate)

### Requirements
- Support BCE and CE dates.
- Support partial precision: Year, Month, Day.
- Support approximate flag.
- Sorting and filtering must work reliably across BCE/CE.

### TimelineDate fields
- Era: BCE | CE
- Year: int (>= 1)
- Month: int? (1-12)
- Day: int? (1-31)
- Precision: Year | Month | Day (Time optional later)
- IsApprox: bool

### SortKey
Store a computed sortable value per TimelineDate to enable:
- ORDER BY chronological
- WHERE date between X and Y

SortKey should preserve:
- BCE dates sort before CE dates.
- Within BCE, larger year means earlier (e.g., 500 BCE < 44 BCE in time).

Implementation detail:
- Persist a numeric SortKey on items for Start and End:
  - StartSortKey (required)
  - EndSortKey (nullable)
- Also store DisplayText components from TimelineDate for rendering.

## Publish behavior
- Draft items are visible only to owner/collaborators (and admin/support).
- Public views show only IsPublished items.

## Search behavior
Search text matches:
- Timeline title/description (timeline lists)
- Item title/description
- Tag names attached to items
- Lane name of item