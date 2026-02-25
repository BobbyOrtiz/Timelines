namespace Timelines.Data.Entities;

public sealed class AttachmentEntity
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public string MediaType { get; set; } = string.Empty;
    public string BlobKey { get; set; } = string.Empty;
    public string? ThumbnailBlobKey { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public DateTimeOffset CreatedUtc { get; set; }

    public TimelineItemEntity Item { get; set; } = null!;
}
