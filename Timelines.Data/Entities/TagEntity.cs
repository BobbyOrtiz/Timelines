namespace Timelines.Data.Entities;

public sealed class TagEntity
{
    public Guid Id { get; set; }
    public Guid TimelineId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }

    public TimelineEntity Timeline { get; set; } = null!;
    public ICollection<ItemTagEntity> ItemTags { get; set; } = new List<ItemTagEntity>();
}
