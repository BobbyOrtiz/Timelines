namespace Timelines.Data.Entities;

public sealed class TimelineEntity
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublic { get; set; }
    public bool IsIndexed { get; set; }
    public int DefaultView { get; set; }
    public int DefaultZoom { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }

    public ICollection<LaneEntity> Lanes { get; set; } = new List<LaneEntity>();
    public ICollection<TagEntity> Tags { get; set; } = new List<TagEntity>();
    public ICollection<TimelineItemEntity> Items { get; set; } = new List<TimelineItemEntity>();
    public ICollection<ShareLinkEntity> ShareLinks { get; set; } = new List<ShareLinkEntity>();
}
