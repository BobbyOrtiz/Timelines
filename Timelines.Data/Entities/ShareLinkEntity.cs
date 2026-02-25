namespace Timelines.Data.Entities;

public sealed class ShareLinkEntity
{
    public Guid Id { get; set; }
    public Guid TimelineId { get; set; }
    public string LinkToken { get; set; } = string.Empty;
    public int AccessLevel { get; set; }
    public bool IsEnabled { get; set; }
    public DateTimeOffset? ExpiresUtc { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }

    public TimelineEntity Timeline { get; set; } = null!;
}
