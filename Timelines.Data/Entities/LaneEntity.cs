namespace Timelines.Data.Entities;

public sealed class LaneEntity
{
    public Guid Id { get; set; }
    public Guid TimelineId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }

    public TimelineEntity Timeline { get; set; } = null!;
    public ICollection<TimelineItemEntity> Items { get; set; } = new List<TimelineItemEntity>();
}
