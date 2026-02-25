namespace Timelines.Shared.Contracts;

public sealed record LaneDto(
    Guid Id,
    Guid TimelineId,
    string Name,
    int SortOrder,
    DateTimeOffset CreatedUtc,
    DateTimeOffset UpdatedUtc);
