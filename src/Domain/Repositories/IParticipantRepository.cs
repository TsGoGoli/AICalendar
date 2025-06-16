using AICalendar.Domain.Common;
using AICalendar.Domain.Entities;
using AICalendar.Domain.Enums;

namespace AICalendar.Domain.Repositories;

/// <summary>
/// Repository interface for Participant entity operations
/// </summary>
public interface IParticipantRepository
{
    /// <summary>
    /// Gets all participants for a specific event
    /// </summary>
    /// <param name="eventId">The event ID</param>
    /// <returns>A list of participants for the event</returns>
    Task<IReadOnlyList<Participant>> GetByEventIdAsync(Guid eventId);
    
    /// <summary>
    /// Gets a specific participant by event ID and user ID
    /// </summary>
    /// <param name="eventId">The event ID</param>
    /// <param name="userId">The user ID</param>
    /// <returns>The participant if found, null otherwise</returns>
    Task<Participant?> GetByEventIdAndUserIdAsync(Guid eventId, Guid userId);
    
    /// <summary>
    /// Adds a participant to an event
    /// </summary>
    /// <param name="participant">The participant to add</param>
    Task<Result> AddAsync(Participant participant);
    
    /// <summary>
    /// Updates a participant's status
    /// </summary>
    /// <param name="participant">The participant to update</param>
    Task<Result> UpdateAsync(Participant participant);
    
    /// <summary>
    /// Removes a participant from an event
    /// </summary>
    /// <param name="eventId">The event ID</param>
    /// <param name="userId">The user ID</param>
    Task<Result> RemoveAsync(Guid eventId, Guid userId);
    
    /// <summary>
    /// Gets all participants for a specific user across all events
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="status">Optional status filter</param>
    /// <returns>A list of participations for the user</returns>
    Task<IReadOnlyList<Participant>> GetByUserIdAsync(Guid userId, ParticipantStatus? status = null);
}