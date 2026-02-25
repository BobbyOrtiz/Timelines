namespace Timelines.Api.Models;

public sealed record CreateLaneRequest(
    string Name,
    int SortOrder);

public sealed record UpdateLaneRequest(
    string Name,
    int SortOrder);
