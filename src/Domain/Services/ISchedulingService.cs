using AICalendar.Domain.Common;
using AICalendar.Domain.Entities;
using AICalendar.Domain.ValueObjects;

namespace AICalendar.Domain.Services;

/// <summary>
/// Interface for the scheduling service that finds available time slots for users
/// </summary>
public interface ISchedulingService
{
    /// <summary>
    /// Finds available time slots for a group of users within a specified time range
    /// </summary>
    /// <param name="users">The list of users to find available slots for</param>
    /// <param name="searchRange">The time range to search within</param>
    /// <param name="duration">The minimum duration needed for the meeting</param>
    /// <param name="maxResults">Maximum number of slots to return</param>
    /// <returns>A list of available time slots sorted by start time</returns>
    Result<IReadOnlyList<DateTimeRange>> FindAvailableSlots(
        IEnumerable<User> users,
        DateTimeRange searchRange,
        TimeSpan duration,
    int maxResults = 100);
}