using Timelines.Shared.Contracts;
using Timelines.Shared.Domain;

namespace Timelines.Api.Models;

public sealed record CreateItemRequest(
    Guid LaneId,
    TimelineItemType Type,
    string Title,
    string? Description,
    TimelineDate StartDate,
    TimelineDate? EndDate,
    int DisplayOrderTiebreaker,
    List<Guid>? TagIds);

public sealed record UpdateItemRequest(
    Guid LaneId,
    TimelineItemType Type,
    string Title,
    string? Description,
    TimelineDate StartDate,
    TimelineDate? EndDate,
    int DisplayOrderTiebreaker,
    List<Guid>? TagIds);
