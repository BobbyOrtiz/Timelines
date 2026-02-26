# Timelines.Api

Azure Functions Isolated API (.NET 10) for the Timelines application.

## Responsibilities

- Auth validation (External ID/B2C) - future step
- CRUD endpoints for timelines/lanes/items/tags/attachments
- Filtering/search endpoints
- Publish/unpublish
- Blob SAS issuance for media
- Import/export (JSON, CSV minimal, poster)

## Current Implementation (Step 3.2)

### Implemented Endpoints

✅ **Health**
- `GET /api/v1/health` - Health check endpoint

✅ **Timelines CRUD**
- `GET /api/v1/timelines` - List user's timelines
- `POST /api/v1/timelines` - Create timeline
- `GET /api/v1/timelines/{timelineId}` - Get timeline by ID
- `PUT /api/v1/timelines/{timelineId}` - Update timeline
- `DELETE /api/v1/timelines/{timelineId}` - Delete timeline

✅ **Lanes CRUD**
- `GET /api/v1/timelines/{timelineId}/lanes` - List lanes for timeline
- `POST /api/v1/timelines/{timelineId}/lanes` - Create lane
- `PUT /api/v1/lanes/{laneId}` - Update lane
- `DELETE /api/v1/lanes/{laneId}` - Delete lane

✅ **Tags CRUD** (NEW)
- `GET /api/v1/timelines/{timelineId}/tags` - List tags for timeline
- `POST /api/v1/timelines/{timelineId}/tags` - Create tag
- `PUT /api/v1/tags/{tagId}` - Update tag
- `DELETE /api/v1/tags/{tagId}` - Delete tag

✅ **Timeline Items CRUD** (NEW)
- `GET /api/v1/timelines/{timelineId}/items` - List/filter items for timeline
- `POST /api/v1/timelines/{timelineId}/items` - Create item
- `GET /api/v1/items/{itemId}` - Get item by ID
- `PUT /api/v1/items/{itemId}` - Update item
- `DELETE /api/v1/items/{itemId}` - Delete item
- `POST /api/v1/items/{itemId}/publish` - Publish item
- `POST /api/v1/items/{itemId}/unpublish` - Unpublish item

#### Filtering Support (GET items)

Query parameters:
- `search` - Search across item title, description, tag names, and lane name
- `tags` - Comma-separated tag IDs (items with ANY of these tags)
- `lanes` - Comma-separated lane IDs
- `types` - Comma-separated TimelineItemType integers (0=Milestone, 1=Period, etc.)
- `fromSortKey` - Filter items with StartSortKey >= value
- `toSortKey` - Filter items with StartSortKey <= value
- `includeDrafts` - true (default for owner) or false (only published items)

Results ordered by: `StartSortKey`, then `DisplayOrderTiebreaker`

### Authentication (Temporary)

Currently using **dev authentication** via `X-Dev-UserId` header:
- Include header: `X-Dev-UserId: <guid>`
- If missing/invalid, defaults to: `11111111-1111-1111-1111-111111111111`
- Will be replaced with proper Azure authentication in a future step

## Setup

### Prerequisites

- .NET 10 SDK
- Azure Functions Core Tools
- SQL Server with Timelines.Dev database (created via migrations)

### Configuration

Connection string is resolved in this order:

1. Environment variable: `TIMELINES_CONNECTION_STRING`
2. `local.settings.json`: `ConnectionStrings:TimelinesDb`
3. Default: `Server=localhost;Database=Timelines.Dev;Trusted_Connection=True;TrustServerCertificate=True;`

### Running Locally

```bash
# From the Timelines.Api directory
func start
```

The API will start on `http://localhost:7071`

## Testing

### Manual Testing with curl

#### 1. Health Check

```bash
curl http://localhost:7071/api/v1/health
```

Expected response:
```json
{
  "status": "ok",
  "timeUtc": "2025-02-25T16:30:00.0000000Z"
}
```

#### 2. Create Timeline

```bash
curl -X POST http://localhost:7071/api/v1/timelines \
  -H "Content-Type: application/json" \
  -H "X-Dev-UserId: 11111111-1111-1111-1111-111111111111" \
  -d "{\"title\":\"Ancient Rome\",\"description\":\"Timeline of Roman Empire events\",\"isPublic\":true,\"isIndexed\":true,\"defaultView\":0,\"defaultZoom\":2}"
```

Expected response (201 Created):
```json
{
  "id": "...",
  "title": "Ancient Rome",
  "description": "Timeline of Roman Empire events",
  "isPublic": true,
  "isIndexed": true,
  "defaultView": 0,
  "defaultZoom": 2,
  "createdUtc": "...",
  "updatedUtc": "..."
}
```

**Save the returned `id` for subsequent requests.**

#### 3. List Timelines

```bash
curl http://localhost:7071/api/v1/timelines \
  -H "X-Dev-UserId: 11111111-1111-1111-1111-111111111111"
```

Expected response (200 OK):
```json
[
  {
    "id": "...",
    "title": "Ancient Rome",
    ...
  }
]
```

#### 4. Get Timeline by ID

```bash
# Replace {timelineId} with actual ID from create response
curl http://localhost:7071/api/v1/timelines/{timelineId} \
  -H "X-Dev-UserId: 11111111-1111-1111-1111-111111111111"
```

#### 5. Update Timeline

```bash
# Replace {timelineId} with actual ID
curl -X PUT http://localhost:7071/api/v1/timelines/{timelineId} \
  -H "Content-Type: application/json" \
  -H "X-Dev-UserId: 11111111-1111-1111-1111-111111111111" \
  -d "{\"title\":\"Ancient Rome (Updated)\",\"description\":\"Updated description\",\"isPublic\":true,\"isIndexed\":false,\"defaultView\":0,\"defaultZoom\":2}"
```

#### 6. Create Lane

```bash
# Replace {timelineId} with actual ID
curl -X POST http://localhost:7071/api/v1/timelines/{timelineId}/lanes \
  -H "Content-Type: application/json" \
  -H "X-Dev-UserId: 11111111-1111-1111-1111-111111111111" \
  -d "{\"name\":\"Political Events\",\"sortOrder\":1}"
```

Expected response (201 Created):
```json
{
  "id": "...",
  "timelineId": "...",
  "name": "Political Events",
  "sortOrder": 1,
  "createdUtc": "...",
  "updatedUtc": "..."
}
```

#### 7. Create Another Lane

```bash
# Replace {timelineId} with actual ID
curl -X POST http://localhost:7071/api/v1/timelines/{timelineId}/lanes \
  -H "Content-Type: application/json" \
  -H "X-Dev-UserId: 11111111-1111-1111-1111-111111111111" \
  -d "{\"name\":\"Military Campaigns\",\"sortOrder\":2}"
```

#### 8. List Lanes

```bash
# Replace {timelineId} with actual ID
curl http://localhost:7071/api/v1/timelines/{timelineId}/lanes \
  -H "X-Dev-UserId: 11111111-1111-1111-1111-111111111111"
```

Expected response (200 OK):
```json
[
  {
    "id": "...",
    "timelineId": "...",
    "name": "Political Events",
    "sortOrder": 1,
    ...
  },
  {
    "id": "...",
    "timelineId": "...",
    "name": "Military Campaigns",
    "sortOrder": 2,
    ...
  }
]
```

#### 9. Update Lane

```bash
# Replace {laneId} with actual ID
curl -X PUT http://localhost:7071/api/v1/lanes/{laneId} \
  -H "Content-Type: application/json" \
  -H "X-Dev-UserId: 11111111-1111-1111-1111-111111111111" \
  -d "{\"name\":\"Political Events (Updated)\",\"sortOrder\":10}"
```

#### 10. Delete Lane

```bash
# Replace {laneId} with actual ID
curl -X DELETE http://localhost:7071/api/v1/lanes/{laneId} \
  -H "X-Dev-UserId: 11111111-1111-1111-1111-111111111111"
```

Expected response: 204 No Content

#### 11. Delete Timeline

```bash
# Replace {timelineId} with actual ID
curl -X DELETE http://localhost:7071/api/v1/timelines/{timelineId} \
  -H "X-Dev-UserId: 11111111-1111-1111-1111-111111111111"
```

Expected response: 204 No Content

### Step 3.2 - Tags and Items Testing

#### 12. Create Tags

```bash
# Replace {timelineId} with actual ID
curl -X POST http://localhost:7071/api/v1/timelines/{timelineId}/tags \
  -H "Content-Type: application/json" \
  -H "X-Dev-UserId: 11111111-1111-1111-1111-111111111111" \
  -d '{"name":"War"}'

curl -X POST http://localhost:7071/api/v1/timelines/{timelineId}/tags \
  -H "Content-Type: application/json" \
  -H "X-Dev-UserId: 11111111-1111-1111-1111-111111111111" \
  -d '{"name":"Politics"}'
```

Save the returned tag IDs.

#### 13. List Tags

```bash
# Replace {timelineId} with actual ID
curl http://localhost:7071/api/v1/timelines/{timelineId}/tags \
  -H "X-Dev-UserId: 11111111-1111-1111-1111-111111111111"
```

#### 14. Create Timeline Item with BCE Date

```bash
# Replace {timelineId} and {laneId} with actual IDs
# This creates an item for the assassination of Julius Caesar (44 BCE, approximate)
curl -X POST http://localhost:7071/api/v1/timelines/{timelineId}/items \
  -H "Content-Type: application/json" \
  -H "X-Dev-UserId: 11111111-1111-1111-1111-111111111111" \
  -d '{
    "laneId": "{laneId}",
    "type": 0,
    "title": "Assassination of Julius Caesar",
    "description": "Caesar was assassinated on the Ides of March",
    "startDate": {
      "era": 0,
      "year": 44,
      "month": 3,
      "day": 15,
      "precision": 2,
      "isApprox": false
    },
    "endDate": null,
    "displayOrderTiebreaker": 1,
    "tagIds": ["{warTagId}", "{politicsTagId}"]
  }'
```

#### 15. Create Item with Year Precision (Approximate)

```bash
# Replace {timelineId} and {laneId} with actual IDs
curl -X POST http://localhost:7071/api/v1/timelines/{timelineId}/items \
  -H "Content-Type: application/json" \
  -H "X-Dev-UserId: 11111111-1111-1111-1111-111111111111" \
  -d '{
    "laneId": "{laneId}",
    "type": 1,
    "title": "Punic Wars",
    "description": "Series of wars between Rome and Carthage",
    "startDate": {
      "era": 0,
      "year": 264,
      "month": null,
      "day": null,
      "precision": 0,
      "isApprox": true
    },
    "endDate": {
      "era": 0,
      "year": 146,
      "month": null,
      "day": null,
      "precision": 0,
      "isApprox": false
    },
    "displayOrderTiebreaker": 1,
    "tagIds": ["{warTagId}"]
  }'
```

#### 16. Create CE Item

```bash
# Replace {timelineId} and {laneId} with actual IDs
curl -X POST http://localhost:7071/api/v1/timelines/{timelineId}/items \
  -H "Content-Type: application/json" \
  -H "X-Dev-UserId: 11111111-1111-1111-1111-111111111111" \
  -d '{
    "laneId": "{laneId}",
    "type": 0,
    "title": "Fall of Western Roman Empire",
    "description": "Romulus Augustus deposed",
    "startDate": {
      "era": 1,
      "year": 476,
      "month": 9,
      "day": 4,
      "precision": 2,
      "isApprox": false
    },
    "endDate": null,
    "displayOrderTiebreaker": 1,
    "tagIds": ["{politicsTagId}"]
  }'
```

#### 17. List All Items

```bash
# Replace {timelineId} with actual ID
curl "http://localhost:7071/api/v1/timelines/{timelineId}/items" \
  -H "X-Dev-UserId: 11111111-1111-1111-1111-111111111111"
```

#### 18. Filter Items by Tag

```bash
# Replace {timelineId} and {warTagId} with actual IDs
curl "http://localhost:7071/api/v1/timelines/{timelineId}/items?tags={warTagId}" \
  -H "X-Dev-UserId: 11111111-1111-1111-1111-111111111111"
```

#### 19. Search Items

```bash
# Replace {timelineId} with actual ID
curl "http://localhost:7071/api/v1/timelines/{timelineId}/items?search=caesar" \
  -H "X-Dev-UserId: 11111111-1111-1111-1111-111111111111"
```

#### 20. Filter by Lane and Type

```bash
# Replace {timelineId} and {laneId} with actual IDs
# Type 0 = Milestone
curl "http://localhost:7071/api/v1/timelines/{timelineId}/items?lanes={laneId}&types=0" \
  -H "X-Dev-UserId: 11111111-1111-1111-1111-111111111111"
```

#### 21. Filter by Sort Key Range (BCE dates)

```bash
# Replace {timelineId} with actual ID
# Get items from 500 BCE to 1 BCE (fromSortKey=-5000000, toSortKey=-10000)
curl "http://localhost:7071/api/v1/timelines/{timelineId}/items?fromSortKey=-5000000&toSortKey=-10000" \
  -H "X-Dev-UserId: 11111111-1111-1111-1111-111111111111"
```

#### 22. Publish Item

```bash
# Replace {itemId} with actual ID
curl -X POST http://localhost:7071/api/v1/items/{itemId}/publish \
  -H "X-Dev-UserId: 11111111-1111-1111-1111-111111111111"
```

#### 23. List Only Published Items

```bash
# Replace {timelineId} with actual ID
curl "http://localhost:7071/api/v1/timelines/{timelineId}/items?includeDrafts=false" \
  -H "X-Dev-UserId: 11111111-1111-1111-1111-111111111111"
```

#### 24. Unpublish Item

```bash
# Replace {itemId} with actual ID
curl -X POST http://localhost:7071/api/v1/items/{itemId}/unpublish \
  -H "X-Dev-UserId: 11111111-1111-1111-1111-111111111111"
```

#### 25. Update Item (Add More Tags)

```bash
# Replace {itemId}, {laneId}, and tag IDs with actual values
curl -X PUT http://localhost:7071/api/v1/items/{itemId} \
  -H "Content-Type: application/json" \
  -H "X-Dev-UserId: 11111111-1111-1111-1111-111111111111" \
  -d '{
    "laneId": "{laneId}",
    "type": 0,
    "title": "Assassination of Julius Caesar (Updated)",
    "description": "Updated description",
    "startDate": {
      "era": 0,
      "year": 44,
      "month": 3,
      "day": 15,
      "precision": 2,
      "isApprox": false
    },
    "endDate": null,
    "displayOrderTiebreaker": 1,
    "tagIds": ["{warTagId}", "{politicsTagId}"]
  }'
```

### PowerShell Testing Examples (Extended)

```powershell
# Complete workflow
$headers = @{
    "Content-Type" = "application/json"
    "X-Dev-UserId" = "11111111-1111-1111-1111-111111111111"
}

# Create timeline
$timeline = @{
    title = "Ancient Rome"
    description = "Timeline of Roman Empire events"
    isPublic = $true
    isIndexed = $true
    defaultView = 0
    defaultZoom = 2
} | ConvertTo-Json

$timelineResult = Invoke-RestMethod -Uri "http://localhost:7071/api/v1/timelines" `
    -Method Post -Headers $headers -Body $timeline
$timelineId = $timelineResult.id

# Create lane
$lane = @{
    name = "Political Events"
    sortOrder = 1
} | ConvertTo-Json

$laneResult = Invoke-RestMethod -Uri "http://localhost:7071/api/v1/timelines/$timelineId/lanes" `
    -Method Post -Headers $headers -Body $lane
$laneId = $laneResult.id

# Create tags
$warTag = @{ name = "War" } | ConvertTo-Json
$warResult = Invoke-RestMethod -Uri "http://localhost:7071/api/v1/timelines/$timelineId/tags" `
    -Method Post -Headers $headers -Body $warTag
$warTagId = $warResult.id

$politicsTag = @{ name = "Politics" } | ConvertTo-Json
$politicsResult = Invoke-RestMethod -Uri "http://localhost:7071/api/v1/timelines/$timelineId/tags" `
    -Method Post -Headers $headers -Body $politicsTag
$politicsTagId = $politicsResult.id

# Create item with BCE date
$item = @{
    laneId = $laneId
    type = 0
    title = "Assassination of Julius Caesar"
    description = "Caesar was assassinated on the Ides of March"
    startDate = @{
        era = 0
        year = 44
        month = 3
        day = 15
        precision = 2
        isApprox = $false
    }
    endDate = $null
    displayOrderTiebreaker = 1
    tagIds = @($warTagId, $politicsTagId)
} | ConvertTo-Json -Depth 5

$itemResult = Invoke-RestMethod -Uri "http://localhost:7071/api/v1/timelines/$timelineId/items" `
    -Method Post -Headers $headers -Body $item
$itemId = $itemResult.id

# List all items
$items = Invoke-RestMethod -Uri "http://localhost:7071/api/v1/timelines/$timelineId/items" `
    -Method Get -Headers @{"X-Dev-UserId" = "11111111-1111-1111-1111-111111111111"}

Write-Host "Created $($items.Count) items"

# Search by tag name
$searchResults = Invoke-RestMethod -Uri "http://localhost:7071/api/v1/timelines/$timelineId/items?search=war" `
    -Method Get -Headers @{"X-Dev-UserId" = "11111111-1111-1111-1111-111111111111"}

# Publish item
Invoke-RestMethod -Uri "http://localhost:7071/api/v1/items/$itemId/publish" `
    -Method Post -Headers @{"X-Dev-UserId" = "11111111-1111-1111-1111-111111111111"}

# Get only published items
$published = Invoke-RestMethod -Uri "http://localhost:7071/api/v1/timelines/$timelineId/items?includeDrafts=false" `
    -Method Get -Headers @{"X-Dev-UserId" = "11111111-1111-1111-1111-111111111111"}
```

## Error Responses

The API returns appropriate HTTP status codes:

- **200 OK** - Successful GET/PUT request
- **201 Created** - Successful POST request
- **204 No Content** - Successful DELETE request
- **400 Bad Request** - Validation errors (missing required fields, invalid format, invalid TimelineDate)
- **404 Not Found** - Resource not found or not owned by user
- **409 Conflict** - Unique constraint violation (duplicate lane/tag name)
- **500 Internal Server Error** - Unexpected errors

## Logging

The API uses **Serilog** for logging with console output. Logs include:
- Request information
- Entity creation/update/deletion
- Errors and warnings

## Next Steps

Future enhancements:
- Implement proper Azure authentication (External ID/B2C)
- Add Attachments and blob SAS endpoints
- Add import/export endpoints
- Add public timeline viewing endpoints
