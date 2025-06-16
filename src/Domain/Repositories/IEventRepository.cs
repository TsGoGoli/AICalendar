using AICalendar.Domain.Common;
using AICalendar.Domain.Entities;
using AICalendar.Domain.Enums;
using AICalendar.Domain.ValueObjects;

namespace AICalendar.Domain.Repositories;

/// <summary>
/// Repository interface for Event entity operations
/// </summary>
public interface IEventRepository
{
    /// <summary>
    /// Gets all events with optional filtering
    /// </summary>
    /// <param name="timeRange">Optional time range filter</param>
    /// <param name="status">Optional event status filter</param>
    /// <returns>A list of events matching the filters</returns>
    Task<IReadOnlyList<Event>> GetAllAsync(DateTimeRange? timeRange = null, EventStatus? status = null);
    
    /// <summary>
    /// Gets an event by ID
    /// </summary>
    /// <param name="id">The event ID</param>
    /// <returns>The event if found, null otherwise</returns>
    Task<Event?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Gets all events for a specific user within a time range
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="timeRange">The time range to search within</param>
    /// <param name="status">Optional event status filter</param>
    /// <returns>A list of events for the user within the specified time range</returns>
    Task<IReadOnlyList<Event>> GetByUserIdAndTimeRangeAsync(
        Guid userId, 
        DateTimeRange timeRange, 
        EventStatus? status = null);
    
    /// <summary>
    /// Creates a new event
    /// </summary>
    /// <param name="event">The event to create</param>
    Task<Result> CreateAsync(Event @event);
    
    /// <summary>
    /// Updates an existing event
    /// </summary>
    /// <param name="event">The event to update</param>
    Task<Result> UpdateAsync(Event @event);
    
    /// <summary>
    /// Deletes an event by ID
    /// </summary>
    /// <param name="id">The ID of the event to delete</param>
    Task<Result> DeleteAsync(Guid id);
}