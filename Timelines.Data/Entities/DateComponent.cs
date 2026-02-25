using Timelines.Shared.Domain;

namespace Timelines.Data.Entities;

/// <summary>
/// Owned type representing a timeline date with BCE/CE support, precision, and approximate flag.
/// </summary>
public sealed class DateComponent
{
    public TimelineEra Era { get; set; }
    public int Year { get; set; }
    public int? Month { get; set; }
    public int? Day { get; set; }
    public TimelineDatePrecision Precision { get; set; }
    public bool IsApprox { get; set; }
}
