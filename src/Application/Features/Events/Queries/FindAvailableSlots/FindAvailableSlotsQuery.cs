using AICalendar.Application.Common.Queries;
using AICalendar.Domain.ValueObjects;

namespace AICalendar.Application.Features.Events.Queries.FindAvailableSlots;

/// <summary>
/// Query to find available time slots for a group of users
/// </summary>
public class FindAvailableSlotsQuery : IQuery<IReadOnlyList<DateTimeRange>>
{
    public List<Guid> UserIds { get; set; } = new List<Guid>();
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public TimeSpan Duration { get; set; }
    public int MaxResults { get; set; } = 5;
    
    public DateTimeRange GetSearchRange()
    {
        return new DateTimeRange(Start, End);
    }
}