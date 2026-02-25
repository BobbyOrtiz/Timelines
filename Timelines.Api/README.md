# Timelines.Api

Azure Functions Isolated API (.NET 10) for the Timelines application.

## Responsibilities

- Auth validation (External ID/B2C) - future step
- CRUD endpoints for timelines/lanes/items/tags/attachments
- Filtering/search endpoints
- Publish/unpublish
- Blob SAS issuance for media
- Import/export (JSON, CSV minimal, poster)

## Current Implementation (Step 3)

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

### PowerShell Testing Examples

```powershell
# Health check
Invoke-RestMethod -Uri "http://localhost:7071/api/v1/health" -Method Get

# Create timeline
$timeline = @{
    title = "Ancient Rome"
    description = "Timeline of Roman Empire events"
    isPublic = $true
    isIndexed = $true
    defaultView = 0
    defaultZoom = 2
} | ConvertTo-Json

$headers = @{
    "Content-Type" = "application/json"
    "X-Dev-UserId" = "11111111-1111-1111-1111-111111111111"
}

$result = Invoke-RestMethod -Uri "http://localhost:7071/api/v1/timelines" `
    -Method Post `
    -Headers $headers `
    -Body $timeline

$timelineId = $result.id

# List timelines
Invoke-RestMethod -Uri "http://localhost:7071/api/v1/timelines" `
    -Method Get `
    -Headers @{"X-Dev-UserId" = "11111111-1111-1111-1111-111111111111"}

# Create lane
$lane = @{
    name = "Political Events"
    sortOrder = 1
} | ConvertTo-Json

$laneResult = Invoke-RestMethod -Uri "http://localhost:7071/api/v1/timelines/$timelineId/lanes" `
    -Method Post `
    -Headers $headers `
    -Body $lane

# List lanes
Invoke-RestMethod -Uri "http://localhost:7071/api/v1/timelines/$timelineId/lanes" `
    -Method Get `
    -Headers @{"X-Dev-UserId" = "11111111-1111-1111-1111-111111111111"}
```

## Error Responses

The API returns appropriate HTTP status codes:

- **200 OK** - Successful GET/PUT request
- **201 Created** - Successful POST request
- **204 No Content** - Successful DELETE request
- **400 Bad Request** - Validation errors (missing required fields, invalid format)
- **404 Not Found** - Resource not found or not owned by user
- **409 Conflict** - Unique constraint violation (duplicate lane name)
- **500 Internal Server Error** - Unexpected errors

## Logging

The API uses **Serilog** for logging with console output. Logs include:
- Request information
- Entity creation/update/deletion
- Errors and warnings

## Next Steps

Future enhancements:
- Implement proper Azure authentication (External ID/B2C)
- Add Tags CRUD endpoints
- Add TimelineItems CRUD endpoints
- Add Attachments and blob SAS endpoints
- Add filtering and search capabilities
- Add publish/unpublish functionality
- Add import/export endpoints
