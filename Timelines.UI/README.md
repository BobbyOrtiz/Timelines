# Timelines.UI

Shared Razor Class Library (RCL) with MudBlazor components for the Timelines application.

## Purpose

Timelines.UI is a **host-agnostic** component library that provides reusable Blazor pages and components for both:
- **Timelines.Web** (Blazor WebAssembly)
- **Timelines.Mobile** (.NET MAUI Blazor Hybrid)

By design, this library contains **no HTTP calls**, **no platform-specific APIs**, and **no ASP.NET dependencies** beyond what's required for Blazor components.

## Architecture

### Design Principles

✅ **Host-Agnostic** - Components use parameters and callbacks, not direct API calls  
✅ **Mobile-First** - Responsive layout using MudBlazor's breakpoint system  
✅ **Pure Blazor** - Only `.razor` files, no Razor Pages  
✅ **MudBlazor-Based** - Leverages MudBlazor components for consistent UI  
✅ **Reusable** - Components can be used in any Blazor host application  

### Component Model

All components follow a **parameter-driven, callback-based** architecture:

```razor
<TimelineDetailPage 
    Timeline="@timeline"
    Items="@items"
    Lanes="@lanes"
    Tags="@tags"
    OnCreateItem="@HandleCreateItem"
    OnEditItem="@HandleEditItem" />
```

The **host application** is responsible for:
- Fetching data from APIs
- Managing application state
- Handling navigation
- Implementing callbacks

The **UI library** is responsible for:
- Rendering components
- Handling user interactions
- Raising events via callbacks
- Validating input

## Folder Structure

```
Timelines.UI/
├── Components/
│   ├── AppShell.razor              - Main layout with app bar and drawer
│   ├── BceDatePicker.razor         - BCE/CE date input with precision
│   ├── FilterPanel.razor           - Multi-criteria filter UI
│   ├── ItemCard.razor              - Timeline item display card
│   ├── FeedView.razor              - Vertical chronological list
│   └── TimelineView.razor          - Horizontal timeline with zoom
├── Pages/
│   ├── TimelinesListPage.razor     - List of user's timelines
│   ├── TimelineDetailPage.razor    - Timeline viewer with filtering
│   └── TimelineItemEditorPage.razor - Create/edit timeline items
└── README.md
```

## Components

### Layout Components

#### **AppShell**
Main application shell with MudBlazor layout.

**Parameters:**
- `ChildContent` - Main page content
- `TopBarContent` - Custom content for the app bar
- `DrawerContent` - Content for the filter drawer

**Example:**
```razor
<AppShell>
    <TopBarContent>
        <MudButton>Settings</MudButton>
    </TopBarContent>
    <DrawerContent>
        <FilterPanel ... />
    </DrawerContent>
    <ChildContent>
        <TimelineDetailPage ... />
    </ChildContent>
</AppShell>
```

### Input Components

#### **BceDatePicker**
Date picker supporting BCE/CE dates with variable precision.

**Parameters:**
- `Value` - `TimelineDate?` - Current date value
- `ValueChanged` - `EventCallback<TimelineDate>` - Raised when date changes

**Features:**
- Era selection (BCE/CE)
- Precision selection (Year/Month/Day)
- Conditional fields based on precision
- Approximate date toggle
- Built-in validation using `TimelineDate.Validate()`

**Example:**
```razor
<BceDatePicker @bind-Value="startDate" />
```

#### **FilterPanel**
Multi-criteria filter panel for timeline items.

**Parameters:**
- `SearchText` - Search query string
- `Lanes` - Available lanes for filtering
- `Tags` - Available tags for filtering
- `Types` - Available item types
- `SelectedLaneIds` - Currently selected lane IDs
- `SelectedTagIds` - Currently selected tag IDs
- `SelectedTypes` - Currently selected types
- `IncludeDrafts` - Whether to include draft items
- Various `EventCallback` parameters for changes

**Example:**
```razor
<FilterPanel 
    SearchText="@searchText"
    Lanes="@lanes"
    Tags="@tags"
    SelectedLaneIds="@selectedLanes"
    OnFiltersChanged="@ApplyFilters" />
```

### Display Components

#### **ItemCard**
Reusable card for displaying a timeline item.

**Parameters:**
- `Item` - `TimelineItemDto` - The item to display
- `LaneName` - Lane name for the item
- `OnEdit` - `EventCallback` - Raised when edit is clicked
- `OnPublishToggle` - `EventCallback` - Raised when publish/unpublish is clicked

**Features:**
- Displays title, date range, lane, description
- Shows publish status badge
- Edit and publish/unpublish actions

#### **FeedView**
Vertical chronological list of timeline items.

**Parameters:**
- `Items` - List of timeline items
- `Lanes` - Available lanes for lane name lookup
- `OnEditItem` - `EventCallback<TimelineItemDto>` - Edit callback
- `OnPublishToggle` - `EventCallback<TimelineItemDto>` - Publish toggle callback

**Features:**
- Simple vertical list layout
- Renders `ItemCard` components
- Mobile-optimized scrolling

#### **TimelineView**
Horizontal timeline visualization with zoom levels.

**Parameters:**
- `Items` - List of timeline items
- `Lanes` - Available lanes
- `ZoomLevel` - `TimelineZoomLevel` (Day/Month/Year/Decade)
- `OnEditItem` - Edit callback
- `OnPublishToggle` - Publish toggle callback

**Features:**
- Groups items into time buckets based on zoom level
- Horizontal scrolling layout
- Collapse/expand for dense areas ("+N more")
- Supports BCE and CE dates
- Responsive bucket sizing

## Pages

### **TimelinesListPage**
Displays a list of the user's timelines.

**Parameters:**
- `Timelines` - `IReadOnlyList<TimelineDto>` - List of timelines
- `OnCreateTimeline` - `EventCallback` - Create new timeline
- `OnOpenTimeline` - `EventCallback<Guid>` - Open timeline by ID

**Usage in host app:**
```razor
<TimelinesListPage 
    Timelines="@timelines"
    OnCreateTimeline="@ShowCreateDialog"
    OnOpenTimeline="@NavigateToTimeline" />
```

### **TimelineDetailPage**
Main timeline viewer with view mode toggle and filtering.

**Parameters:**
- `Timeline` - Current timeline metadata
- `Items` - Timeline items to display
- `Lanes` - Available lanes
- `Tags` - Available tags
- `ViewMode` - `TimelineViewMode` (Timeline/Feed)
- `ZoomLevel` - `TimelineZoomLevel` (for Timeline view)
- `OnCreateItem` - Create new item callback
- `OnEditItem` - Edit item callback
- `OnPublishToggle` - Publish/unpublish callback
- View mode and zoom level change callbacks

**Features:**
- Toggle between Timeline and Feed views
- Zoom controls (Decade/Year/Month/Day)
- Create item button
- Displays timeline metadata

### **TimelineItemEditorPage**
Create or edit a timeline item.

**Parameters:**
- `Item` - `TimelineItemDto?` - Item to edit (null for create)
- `Lanes` - Available lanes
- `Tags` - Available tags
- `OnSave` - `EventCallback<TimelineItemDto>` - Save callback
- `OnCancel` - `EventCallback` - Cancel callback

**Features:**
- Title, description, lane, type inputs
- Start and end date pickers (BCE/CE support)
- "Ongoing" toggle for items without end dates
- Tag multi-select
- Display order configuration
- Publish toggle (for existing items)
- Validation before save

**Usage:**
```razor
<!-- Create new item -->
<TimelineItemEditorPage 
    Lanes="@lanes"
    Tags="@tags"
    OnSave="@CreateItem"
    OnCancel="@CloseEditor" />

<!-- Edit existing item -->
<TimelineItemEditorPage 
    Item="@selectedItem"
    Lanes="@lanes"
    Tags="@tags"
    OnSave="@UpdateItem"
    OnCancel="@CloseEditor" />
```

## How Host Apps Should Use This Library

### 1. Add Project Reference

```xml
<ProjectReference Include="..\Timelines.UI\Timelines.UI.csproj" />
```

### 2. Configure MudBlazor Services (in Program.cs)

```csharp
builder.Services.AddMudServices();
```

### 3. Add MudBlazor CSS and JS (in index.html or _Host.cshtml)

```html
<link href="https://fonts.googleapis.com/css?family=Roboto:300,400,500,700&display=swap" rel="stylesheet" />
<link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />
<script src="_content/MudBlazor/MudBlazor.min.js"></script>
```

### 4. Implement Host Logic

**Example: Timelines.Web (Blazor WASM)**

```csharp
@page "/timeline/{id:guid}"
@inject HttpClient Http
@inject NavigationManager Nav

<TimelineDetailPage 
    Timeline="@_timeline"
    Items="@_items"
    Lanes="@_lanes"
    Tags="@_tags"
    ViewMode="@_viewMode"
    ZoomLevel="@_zoomLevel"
    OnCreateItem="@CreateItem"
    OnEditItem="@EditItem"
    OnPublishToggle="@TogglePublish" />

@code {
    private TimelineDto? _timeline;
    private List<TimelineItemDto> _items = new();
    private List<LaneDto> _lanes = new();
    private List<TagDto> _tags = new();
    private TimelineViewMode _viewMode = TimelineViewMode.Timeline;
    private TimelineZoomLevel _zoomLevel = TimelineZoomLevel.Year;

    protected override async Task OnInitializedAsync()
    {
        // Host app fetches data from API
        _timeline = await Http.GetFromJsonAsync<TimelineDto>($"/api/v1/timelines/{Id}");
        _items = await Http.GetFromJsonAsync<List<TimelineItemDto>>($"/api/v1/timelines/{Id}/items");
        _lanes = await Http.GetFromJsonAsync<List<LaneDto>>($"/api/v1/timelines/{Id}/lanes");
        _tags = await Http.GetFromJsonAsync<List<TagDto>>($"/api/v1/timelines/{Id}/tags");
    }

    private async Task CreateItem()
    {
        Nav.NavigateTo($"/timeline/{Id}/item/create");
    }

    private async Task EditItem(TimelineItemDto item)
    {
        Nav.NavigateTo($"/item/{item.Id}/edit");
    }

    private async Task TogglePublish(TimelineItemDto item)
    {
        var endpoint = item.IsPublished ? "unpublish" : "publish";
        await Http.PostAsync($"/api/v1/items/{item.Id}/{endpoint}", null);
        await RefreshItems();
    }
}
```

## Important Notes

### No HTTP Calls by Design

This library intentionally **does not contain any HTTP client code** or API calls. This design decision ensures:

- ✅ Host applications control data fetching strategy
- ✅ Components remain testable in isolation
- ✅ Works with any backend (not just the Timelines API)
- ✅ Supports offline scenarios (MAUI)
- ✅ Enables state management flexibility

### No Platform-Specific Code

The library contains **no MAUI-specific** or **Web-specific** code:

- ❌ No `Microsoft.AspNetCore.App` framework reference
- ❌ No MAUI lifecycle hooks
- ❌ No platform detection
- ✅ Pure Blazor components

### Dependencies

- **MudBlazor** (9.0.0) - UI component library
- **Timelines.Shared** - Domain types and DTOs

## Future Enhancements

Planned additions (not in Step 4):
- Attachment gallery components
- Import/Export UI
- Share dialog component
- Localization resources (en/es)
- Timeline poster export UI
- Advanced filtering UI

## Testing

Components can be tested using **bUnit** or rendered in isolation:

```csharp
[Fact]
public void ItemCard_DisplaysCorrectData()
{
    using var ctx = new TestContext();
    var item = new TimelineItemDto(...);

    var component = ctx.RenderComponent<ItemCard>(parameters => parameters
        .Add(p => p.Item, item)
        .Add(p => p.LaneName, "Test Lane"));

    component.Find("h6").TextContent.Should().Be(item.Title);
}
```

## Contributing

When adding new components:

1. ✅ Use parameters and callbacks (no direct API calls)
2. ✅ Follow MudBlazor patterns
3. ✅ Support mobile-first responsive layout
4. ✅ Include XML doc comments
5. ✅ Update this README

---

**Version:** Step 4 Implementation  
**Last Updated:** 2025-02-25  
**MudBlazor Version:** 9.0.0  
**Target Framework:** .NET 10
