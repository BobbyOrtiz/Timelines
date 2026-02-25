namespace Timelines.Data.Entities;

public sealed class TimelineItemEntity
{
    public Guid Id { get; set; }
    public Guid TimelineId { get; set; }
    public Guid LaneId { get; set; }
    public int Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateComponent StartDate { get; set; } = new();
    public DateComponent? EndDate { get; set; }
    public long StartSortKey { get; set; }
    public long? EndSortKey { get; set; }
    public int DisplayOrderTiebreaker { get; set; }
    public bool IsPublished { get; set; }
    public DateTimeOffset? PublishedUtc { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }

    public TimelineEntity Timeline { get; set; } = null!;
    public LaneEntity Lane { get; set; } = null!;
    public ICollection<AttachmentEntity> Attachments { get; set; } = new List<AttachmentEntity>();
    public ICollection<ItemTagEntity> ItemTags { get; set; } = new List<ItemTagEntity>();
}
