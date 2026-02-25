namespace Timelines.Shared.Contracts;

public sealed record TimelineDto(
    Guid Id,
    string Title,
    string? Description,
    bool IsPublic,
    bool IsIndexed,
    TimelineViewMode DefaultView,
    TimelineZoomLevel DefaultZoom,
    DateTimeOffset CreatedUtc,
    DateTimeOffset UpdatedUtc);
