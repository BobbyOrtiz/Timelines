using Timelines.Shared.Domain;

namespace Timelines.Shared.Contracts;

public sealed record TimelineItemDto(
    Guid Id,
    Guid TimelineId,
    Guid LaneId,
    TimelineItemType Type,
    string Title,
    string? Description,
    TimelineDate StartDate,
    TimelineDate? EndDate,
    bool IsPublished,
    DateTimeOffset? PublishedUtc,
    int DisplayOrderTiebreaker,
    DateTimeOffset CreatedUtc,
    DateTimeOffset UpdatedUtc);
