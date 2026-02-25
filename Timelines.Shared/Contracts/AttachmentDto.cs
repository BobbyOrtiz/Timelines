namespace Timelines.Shared.Contracts;

public sealed record AttachmentDto(
    Guid Id,
    Guid ItemId,
    string MediaType,
    string BlobKey,
    string? ThumbnailBlobKey,
    string ContentType,
    long SizeBytes,
    string OriginalFileName,
    DateTimeOffset CreatedUtc);
