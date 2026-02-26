using Timelines.Data.Entities;
using Timelines.Shared.Domain;

namespace Timelines.Api.Mappers;

/// <summary>
/// Maps between Timelines.Shared.Domain.TimelineDate and Timelines.Data.Entities.DateComponent.
/// </summary>
public static class DateMapper
{
    public static DateComponent ToDateComponent(TimelineDate date)
    {
        return new DateComponent
        {
            Era = date.Era,
            Year = date.Year,
            Month = date.Month,
            Day = date.Day,
            Precision = date.Precision,
            IsApprox = date.IsApprox
        };
    }

    public static TimelineDate ToTimelineDate(DateComponent component)
    {
        return new TimelineDate(
            component.Era,
            component.Year,
            component.Month,
            component.Day,
            component.Precision,
            component.IsApprox);
    }
}
