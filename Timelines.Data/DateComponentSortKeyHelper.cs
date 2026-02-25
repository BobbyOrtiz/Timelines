using Timelines.Data.Entities;
using Timelines.Shared.Domain;

namespace Timelines.Data;

/// <summary>
/// Internal helper for computing sort keys from DateComponent entities.
/// Uses the same logic as TimelineSortKey from Timelines.Shared.Domain.
/// </summary>
internal static class DateComponentSortKeyHelper
{
    /// <summary>
    /// Computes a sortable long integer from a DateComponent.
    /// BCE years are mapped to negative integers.
    /// Format: year * 10,000 + month * 100 + day (month/day 0 if null)
    /// </summary>
    public static long ComputeSortKey(DateComponent dateComponent)
    {
        var yearValue = dateComponent.Era == TimelineEra.Bce 
            ? -dateComponent.Year 
            : dateComponent.Year;
        
        var month = dateComponent.Month ?? 0;
        var day = dateComponent.Day ?? 0;
        
        return (long)yearValue * 10_000 + month * 100 + day;
    }
}
