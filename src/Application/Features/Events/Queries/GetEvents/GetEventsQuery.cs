using AICalendar.Application.Common.Queries;
using AICalendar.Application.DTOs;
using AICalendar.Domain.Enums;
using AICalendar.Domain.ValueObjects;

namespace AICalendar.Application.Features.Events.Queries.GetEvents;

/// <summary>
/// Query to get a list of events with optional filtering
/// </summary>
public class GetEventsQuery : IQuery<IReadOnlyList<EventDto>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public EventStatus? Status { get; set; }
    
    /// <summary>
    /// Creates a time range filter if both start and end dates are provided
    /// </summary>
    public DateTimeRange? GetTimeRangeFilter()
    {
        if (StartDate.HasValue && EndDate.HasValue)
        {
            return new DateTimeRange(StartDate.Value, EndDate.Value);
        }
        
        return null;
    }
}