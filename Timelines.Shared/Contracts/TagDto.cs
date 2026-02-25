namespace Timelines.Shared.Contracts;

public sealed record TagDto(
    Guid Id,
    Guid TimelineId,
    string Name,
    DateTimeOffset CreatedUtc,
    DateTimeOffset UpdatedUtc);
