namespace Timelines.Shared.Domain;

/// <summary>
/// Provides deterministic ordering for TimelineDate instances.
/// Note: This is a sorting key utility, not a calendrical conversion.
/// </summary>
public static class TimelineSortKey
{
    /// <summary>
    /// Converts a TimelineDate to a sortable long integer.
    /// BCE years are mapped to negative integers.
    /// </summary>
    public static long ToSortKey(TimelineDate date)
    {
        // Map BCE years to negative, CE to positive
        var yearValue = date.Era == TimelineEra.Bce ? -date.Year : date.Year;
        
        var month = date.Month ?? 0;
        var day = date.Day ?? 0;
        
        // Encode: year * 10,000 + month * 100 + day
        return (long)yearValue * 10_000 + month * 100 + day;
    }

    /// <summary>
    /// Compares two TimelineDate instances for ordering.
    /// First compares by sort key, then by precision if keys are equal.
    /// </summary>
    public static int Compare(TimelineDate a, TimelineDate b)
    {
        var keyA = ToSortKey(a);
        var keyB = ToSortKey(b);
        
        if (keyA != keyB)
            return keyA.CompareTo(keyB);
        
        // If sort keys are equal, compare by precision (Year < Month < Day)
        return a.Precision.CompareTo(b.Precision);
    }
}
