# Timelines

Timelines is a .NET 10 application for creating and visualizing timelines with:
- Point-in-time and range items (open-ended ranges supported)
- BCE/CE dates and approximate dates with explicit precision
- Lanes (tracks), tags, item types, and rich filtering (search + tags + lanes + types + date range)
- Media attachments (images/videos) stored in Azure Blob Storage
- Sharing via link (unlisted or public/indexed), with public viewing not requiring login
- Draft vs Published items (public views show Published only)
- Localization for UI (English/Spanish), user-selected language

## Projects
- **Timelines.Shared**: shared domain + DTOs + shared constants
- **Timelines.Data**: EF Core data model, DbContext, migrations, query helpers
- **Timelines.Api**: Azure Functions (isolated) API used by Web/Mobile/Admin
- **Timelines.UI**: shared MudBlazor UI components used by Web + Mobile
- **Timelines.Web**: Blazor WebAssembly client
- **Timelines.Mobile**: .NET MAUI Blazor Hybrid client
- **Timelines.Admin**: Blazor Server admin/support app (user management + CRUD)

## Hosting (target)
- API + Web hosted in Azure App Services (Functions isolated for API)
- SQL Server locally, Azure SQL in production
- Blob Storage for media (private containers + short-lived SAS issued by API)
- Auth via Entra External ID / Azure AD B2C, passwordless OTP via Azure Communication Services (Email + WhatsApp)

## Docs
See `docs/`:
- `spec.md` - product requirements and scope
- `domain-model.md` - domain + date model (BCE/approx/precision)
- `auth.md` - auth flows and token strategy
- `api.md` - endpoint inventory + filtering contracts
- `storage.md` - blob layout + SAS + thumbnails
- `ui-workflow.md` - UX flows + views + zoom behavior
- `architecture.md` - solution boundaries + dependencies
- `instructions.md` - Copilot build plan + conventions

## Development notes
- All services use `ILogger` and Serilog sinks.
- Prefer mobile-first responsive UI via MudBlazor components in `Timelines.UI`.