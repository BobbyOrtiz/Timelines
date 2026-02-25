# Storage (Blob + Media)

## Goals
- Store images/videos for items.
- Keep blobs private.
- Support public viewing of published items without making containers public.
- Use API-issued short-lived SAS for read and write.

## Container strategy
- Single container: timelines-media (private)
  - Alternatively per-environment containers: timelines-media-dev, timelines-media-prod.

## Blob key format
/timelines/{timelineId}/items/{itemId}/{attachmentId}/{filename}
/timelines/{timelineId}/items/{itemId}/{attachmentId}/{filename}_thumb.jpg

## Upload
1. Client requests upload SAS from API for a specific blob key.
2. Client uploads directly to Blob using SAS.
3. Client notifies API to finalize metadata (size, content-type, optional dimensions).

## Read
- Client requests read SAS from API for an attachment.
- API verifies authorization based on:
  - Owner/collaborator access OR
  - Timeline is public/unlisted AND item is published AND share token valid (if unlisted)

## Thumbnails (v1 recommendation)
- Images: generate a thumbnail (e.g., max 200px) and store alongside original.
- Video thumbnails optional later.

## Security
- SAS tokens are short-lived.
- SAS scope is least privilege:
  - Upload SAS: write only to a specific blob key
  - Read SAS: read only to a specific blob key

## Notes
- For poster export images/PDFs, generate server-side and store temporarily or stream directly.