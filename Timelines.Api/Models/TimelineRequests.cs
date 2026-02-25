using Timelines.Shared.Contracts;

namespace Timelines.Api.Models;

public sealed record CreateTimelineRequest(
    string Title,
    string? Description,
    bool IsPublic,
    bool IsIndexed,
    TimelineViewMode DefaultView,
    TimelineZoomLevel DefaultZoom);

public sealed record UpdateTimelineRequest(
    string Title,
    string? Description,
    bool IsPublic,
    bool IsIndexed,
    TimelineViewMode DefaultView,
    TimelineZoomLevel DefaultZoom);
