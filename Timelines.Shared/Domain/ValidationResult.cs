namespace Timelines.Shared.Domain;

/// <summary>
/// Represents the result of a validation operation.
/// </summary>
public sealed record ValidationResult(bool IsValid, string? Error);
