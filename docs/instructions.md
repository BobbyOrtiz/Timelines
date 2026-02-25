# Copilot Build Instructions (v1)

## Constraints
- .NET 10, C#
- Azure Functions Isolated for API
- SQL Server local, Azure SQL in prod
- Use EF Core in Timelines.Data
- Use ILogger everywhere; configure Serilog per app
- UI components in Timelines.UI using MudBlazor
- Localization: UI only (en/es)
- Timeline items: draft/published flag
- Dates: BCE/CE, precision, approx, open-ended ranges

## Build order
1) Timelines.Shared
   - Define TimelineDate and validation rules
   - Define DTOs for timelines/lanes/items/tags/attachments
   - Define enums/constants for zoom, view, access levels, item types

2) Timelines.Data
   - Define EF entities and DbContext
   - Implement SortKey approach for Start/End (persisted fields on Item)
   - Add migrations and local dev initialization

3) Timelines.Api
   - Wire DI, DbContext, Serilog
   - Implement auth validation (B2C/External ID)
   - Implement endpoints in api.md order
   - Implement filtering/search (joins on tags + lanes)
   - Implement publish toggles (IsPublished, PublishedUtc)
   - Implement blob SAS issuance endpoints

4) Timelines.UI
   - Build reusable components:
     - TimelineView
     - FeedView
     - FilterPanel
     - BCE Date Picker component
     - Attachment gallery

5) Timelines.Web + Timelines.Mobile
   - Implement auth screens (email/whatsapp OTP flows)
   - Implement timeline list + timeline detail view
   - Implement create/edit item flows

6) Timelines.Admin
   - Role-based access (support staff)
   - CRUD screens for all tables
   - Search tools for support

## Quality gates
- Unit tests for SortKey/date ordering rules.
- Basic integration test for filtering.
- Ensure public timeline view works without login.
- Ensure private blobs are accessible only via API-issued SAS.

## Non-goals
- No moderation queue
- No offline mode
- No publish version history