# Architecture

## High-level
Clients (Web + Mobile) and Admin call a shared API (Azure Functions isolated). Data access lives in Timelines.Data. Shared contracts and domain primitives live in Timelines.Shared. Shared UI components (MudBlazor) live in Timelines.UI.

## Project responsibilities
- Timelines.Shared
  - Domain primitives: TimelineDate, precision, BCE/CE handling
  - DTOs for API requests/responses
  - Shared constants, validation helpers

- Timelines.Data
  - EF Core DbContext + migrations
  - Entity models aligned with Domain Model
  - Query helpers for filtering/sorting (especially BCE date sorting via SortKey)

- Timelines.Api (Functions isolated)
  - Auth integration / token validation
  - CRUD for timelines/lanes/items/tags/attachments
  - Search/filter endpoints
  - Blob SAS issuance endpoints
  - Import/export endpoints

- Timelines.UI (RCL)
  - Shared MudBlazor components
  - Timeline visualization components + filter panels
  - Localization resources for UI

- Timelines.Web (WASM)
  - Auth UI + session
  - Uses Timelines.UI
  - Calls API

- Timelines.Mobile (MAUI Blazor Hybrid)
  - Auth UI + secure token storage
  - Uses Timelines.UI
  - Calls API

- Timelines.Admin (Blazor Server)
  - Support staff role-based access
  - User management + CRUD views for tables
  - Calls API for operations OR directly uses Data (choose one pattern and stick to it)

## Data flow
Client -> API -> Data (EF Core) -> SQL Server/Azure SQL
Client -> API -> Blob SAS -> Blob Storage

## Security model
- Private blob containers.
- API issues short-lived SAS tokens for specific blob paths based on authorization.
- Public timelines still use SAS issuance so blob containers remain private.

## Localization
- UI-only localization using .resx or equivalent.
- User preference stored on profile.