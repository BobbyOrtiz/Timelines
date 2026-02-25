# API (Azure Functions Isolated)

## Principles
- All endpoints are versioned (e.g., /api/v1).
- Auth required for owner operations.
- Public viewing supports ShareLink token without auth.
- Filtering endpoints support: search text, tags, lanes, types, date range.

## Public endpoints (no login)
- GET /api/v1/public/timelines/{timelineId}
  - Requires: timeline is public OR valid share token
  - Returns: timeline metadata + lanes + published items only

- GET /api/v1/public/timelines/by-link/{linkToken}
  - Returns: timeline metadata + lanes + published items only (or per link access)

## Authenticated endpoints
### Timelines
- GET /api/v1/timelines
- POST /api/v1/timelines
- GET /api/v1/timelines/{timelineId}
- PUT /api/v1/timelines/{timelineId}
- DELETE /api/v1/timelines/{timelineId}

### Lanes
- GET /api/v1/timelines/{timelineId}/lanes
- POST /api/v1/timelines/{timelineId}/lanes
- PUT /api/v1/lanes/{laneId}
- DELETE /api/v1/lanes/{laneId}

### Tags
- GET /api/v1/timelines/{timelineId}/tags
- POST /api/v1/timelines/{timelineId}/tags
- PUT /api/v1/tags/{tagId}
- DELETE /api/v1/tags/{tagId}

### Items
- GET /api/v1/timelines/{timelineId}/items?search=&tags=&lanes=&types=&from=&to=&includeDrafts=
- POST /api/v1/timelines/{timelineId}/items
- GET /api/v1/items/{itemId}
- PUT /api/v1/items/{itemId}
- DELETE /api/v1/items/{itemId}

### Publish
- POST /api/v1/timelines/{timelineId}/publish
  - publishes selected items or all drafts based on payload
- POST /api/v1/items/{itemId}/publish
- POST /api/v1/items/{itemId}/unpublish

### Attachments & media
- POST /api/v1/items/{itemId}/attachments (metadata create)
- POST /api/v1/items/{itemId}/attachments/upload-sas (get SAS for upload)
- GET /api/v1/attachments/{attachmentId}/read-sas (get SAS for viewing)
- DELETE /api/v1/attachments/{attachmentId}

### Import/Export
- GET /api/v1/timelines/{timelineId}/export/json
- POST /api/v1/timelines/{timelineId}/import/json
- POST /api/v1/timelines/{timelineId}/import/csv-minimal
- GET /api/v1/timelines/{timelineId}/export/poster (PDF/PNG)

## Filtering contract
- search: matches item title/description + tag names + lane name
- tags: tag ids or names (decide in implementation; ids preferred)
- lanes: lane ids
- types: type values
- from/to: TimelineDate range (implemented using SortKey comparisons)
- includeDrafts:
  - true for owner/collaborator views
  - false for public views

## Error conventions
- 400 for validation
- 401 for unauthenticated
- 403 for unauthorized
- 404 for not found