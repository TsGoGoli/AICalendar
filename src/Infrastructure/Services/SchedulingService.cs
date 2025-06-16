using AICalendar.Domain.Common;
using AICalendar.Domain.Entities;
using AICalendar.Domain.Enums;
using AICalendar.Domain.Repositories;
using AICalendar.Domain.Services;
using AICalendar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace AICalendar.Infrastructure.Services;

/// <summary>
/// Implementation of ISchedulingService
/// </summary>
public class SchedulingService : ISchedulingService
{
    private readonly IEventRepository _eventRepository;
    private readonly ILogger<SchedulingService> _logger;

    public SchedulingService(IEventRepository eventRepository, ILogger<SchedulingService> logger)
    {
        _eventRepository = eventRepository;
        _logger = logger;
    }

    public Result<IReadOnlyList<DateTimeRange>> FindAvailableSlots(
        IEnumerable<User> users,
        DateTimeRange searchRange,
        TimeSpan duration,
        int maxResults = 5)
    {
        if (users == null || !users.Any())
            return Result.Failure<IReadOnlyList<DateTimeRange>>("At least one user must be specified");

        if (searchRange == null)
            return Result.Failure<IReadOnlyList<DateTimeRange>>("Search range must be specified");

        if (duration <= TimeSpan.Zero)
            return Result.Failure<IReadOnlyList<DateTimeRange>>("Duration must be greater than zero");

        if (duration > searchRange.Duration)
            return Result.Failure<IReadOnlyList<DateTimeRange>>("Duration cannot be longer than search range");

        if (maxResults <= 0)
            return Result.Failure<IReadOnlyList<DateTimeRange>>("Maximum results must be greater than zero");

        try
        {
            var availableSlots = FindAvailableSlotsAsync(users, searchRange, duration, maxResults).GetAwaiter().GetResult();
            return Result.Success<IReadOnlyList<DateTimeRange>>(availableSlots);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find available slots");
            return Result.Failure<IReadOnlyList<DateTimeRange>>($"Failed to find available slots: {ex.Message}");
        }
    }

    private async Task<List<DateTimeRange>> FindAvailableSlotsAsync(
        IEnumerable<User> users,
        DateTimeRange searchRange,
        TimeSpan duration,
        int maxResults)
    {
        // Get all existing events for all users within the search range
        var userIds = users.Select(u => u.Id).ToList();
        var allEvents = new List<Event>();

        foreach (var userId in userIds)
        {
            var userEvents = await _eventRepository.GetByUserIdAndTimeRangeAsync(
                userId,
                searchRange,
                EventStatus.Scheduled); // Only consider scheduled events, not cancelled ones
                
            allEvents.AddRange(userEvents);
        }

        // Extract all busy time ranges
        var busyRanges = allEvents
            .Select(e => e.TimeRange)
            .OrderBy(tr => tr.Start)
            .ToList();

        // Merge overlapping busy ranges
        var mergedBusyRanges = MergeOverlappingRanges(busyRanges);

        // Find available slots
        var availableSlots = new List<DateTimeRange>();
        DateTime currentStart = searchRange.Start;

        foreach (var busyRange in mergedBusyRanges)
        {
            // If there's enough time before this busy period, add it as an available slot
            if (busyRange.Start - currentStart >= duration)
            {
                availableSlots.Add(new DateTimeRange(currentStart, busyRange.Start));
            }
            
            // Move the current time pointer to after this busy period
            currentStart = busyRange.End > currentStart ? busyRange.End : currentStart;
        }

        // Check if there's still time after the last busy period
        if (searchRange.End - currentStart >= duration)
        {
            availableSlots.Add(new DateTimeRange(currentStart, searchRange.End));
        }

        // If no events were found, the entire search range is available
        if (!busyRanges.Any() && searchRange.Duration >= duration)
        {
            availableSlots.Add(searchRange);
        }

        // Optionally split into smaller chunks of exact duration
        var resultSlots = SplitIntoExactDurations(availableSlots, duration);

        // Take only the requested number of slots
        return resultSlots.Take(maxResults).ToList();
    }

    private List<DateTimeRange> MergeOverlappingRanges(List<DateTimeRange> ranges)
    {
        if (!ranges.Any())
            return new List<DateTimeRange>();

        // Sort by start time
        var sortedRanges = ranges.OrderBy(r => r.Start).ToList();
        var result = new List<DateTimeRange> { sortedRanges[0] };

        for (int i = 1; i < sortedRanges.Count; i++)
        {
            var current = sortedRanges[i];
            var lastMerged = result[result.Count - 1];

            if (current.Start <= lastMerged.End)
            {
                // Ranges overlap, merge them
                var mergedEnd = current.End > lastMerged.End ? current.End : lastMerged.End;
                result[result.Count - 1] = new DateTimeRange(lastMerged.Start, mergedEnd);
            }
            else
            {
                // No overlap, add as a new range
                result.Add(current);
            }
        }

        return result;
    }

    private List<DateTimeRange> SplitIntoExactDurations(List<DateTimeRange> availableRanges, TimeSpan duration)
    {
        var result = new List<DateTimeRange>();

        foreach (var range in availableRanges)
        {
            var start = range.Start;
            while (start.Add(duration) <= range.End)
            {
                result.Add(new DateTimeRange(start, start.Add(duration)));
                
                // Move start time by 30 minutes to offer more options
                // This creates overlapping options but provides more flexibility
                start = start.AddMinutes(30);
            }
        }

        return result.OrderBy(r => r.Start).ToList();
    }
}