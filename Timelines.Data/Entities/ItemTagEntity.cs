namespace Timelines.Data.Entities;

public sealed class ItemTagEntity
{
    public Guid ItemId { get; set; }
    public Guid TagId { get; set; }

    public TimelineItemEntity Item { get; set; } = null!;
    public TagEntity Tag { get; set; } = null!;
}
