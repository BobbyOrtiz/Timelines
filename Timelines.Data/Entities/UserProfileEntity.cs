namespace Timelines.Data.Entities;

public sealed class UserProfileEntity
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string PreferredLanguage { get; set; } = "en";
    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
}
