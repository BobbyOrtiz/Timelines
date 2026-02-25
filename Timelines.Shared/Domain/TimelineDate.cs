namespace Timelines.Shared.Domain;

/// <summary>
/// Represents a date in a timeline with support for different precision levels and approximate dates.
/// </summary>
public sealed record TimelineDate(
    TimelineEra Era,
    int Year,
    int? Month,
    int? Day,
    TimelineDatePrecision Precision,
    bool IsApprox)
{
    /// <summary>
    /// Validates the timeline date according to precision and calendar rules.
    /// </summary>
    public ValidationResult Validate()
    {
        if (Year < 1)
            return new ValidationResult(false, "Year must be >= 1");

        if (Month.HasValue && (Month.Value < 1 || Month.Value > 12))
            return new ValidationResult(false, "Month must be between 1 and 12");

        if (Day.HasValue && (Day.Value < 1 || Day.Value > 31))
            return new ValidationResult(false, "Day must be between 1 and 31");

        switch (Precision)
        {
            case TimelineDatePrecision.Year:
                if (Month.HasValue || Day.HasValue)
                    return new ValidationResult(false, "Month and Day must be null when Precision is Year");
                break;

            case TimelineDatePrecision.Month:
                if (!Month.HasValue)
                    return new ValidationResult(false, "Month is required when Precision is Month");
                if (Day.HasValue)
                    return new ValidationResult(false, "Day must be null when Precision is Month");
                break;

            case TimelineDatePrecision.Day:
                if (!Month.HasValue)
                    return new ValidationResult(false, "Month is required when Precision is Day");
                if (!Day.HasValue)
                    return new ValidationResult(false, "Day is required when Precision is Day");
                break;
        }

        return new ValidationResult(true, null);
    }

    /// <summary>
    /// Returns a human-readable display string for the date.
    /// </summary>
    public string ToDisplayString()
    {
        var approxPrefix = IsApprox ? "~ " : "";
        var eraSuffix = Era == TimelineEra.Bce ? " BCE" : "";

        return Precision switch
        {
            TimelineDatePrecision.Year => $"{approxPrefix}{Year}{eraSuffix}",
            TimelineDatePrecision.Month => $"{approxPrefix}{GetMonthAbbreviation(Month!.Value)} {Year}{eraSuffix}",
            TimelineDatePrecision.Day => $"{approxPrefix}{GetMonthAbbreviation(Month!.Value)} {Day}, {Year}{eraSuffix}",
            _ => $"{approxPrefix}{Year}{eraSuffix}"
        };
    }

    private static string GetMonthAbbreviation(int month) => month switch
    {
        1 => "Jan",
        2 => "Feb",
        3 => "Mar",
        4 => "Apr",
        5 => "May",
        6 => "Jun",
        7 => "Jul",
        8 => "Aug",
        9 => "Sep",
        10 => "Oct",
        11 => "Nov",
        12 => "Dec",
        _ => ""
    };
}
