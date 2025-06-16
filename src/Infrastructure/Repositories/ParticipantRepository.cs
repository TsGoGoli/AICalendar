using AICalendar.Domain.Common;
using AICalendar.Domain.Entities;
using AICalendar.Domain.Enums;
using AICalendar.Domain.Repositories;
using AICalendar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AICalendar.Infrastructure.Repositories;

/// <summary>
/// Implementation of IParticipantRepository using Entity Framework Core
/// </summary>
public class ParticipantRepository : IParticipantRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ParticipantRepository> _logger;

    public ParticipantRepository(ApplicationDbContext context, ILogger<ParticipantRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Participant>> GetByEventIdAsync(Guid eventId)
    {
        try
        {
            return await _context.Participants
                .Include(p => p.User)
                .Where(p => p.EventId == eventId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting participants for event {EventId}", eventId);
            throw;
        }
    }

    public async Task<Participant?> GetByEventIdAndUserIdAsync(Guid eventId, Guid userId)
    {
        try
        {
            return await _context.Participants
                .Include(p => p.User)
                .Include(p => p.Event)
                .FirstOrDefaultAsync(p => p.EventId == eventId && p.UserId == userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting participant for event {EventId} and user {UserId}", eventId, userId);
            throw;
        }
    }

    public async Task<Result> AddAsync(Participant participant)
    {
        try
        {
            // Check if participant already exists
            bool exists = await _context.Participants
                .AnyAsync(p => p.EventId == participant.EventId && p.UserId == participant.UserId);
                
            if (exists)
            {
                return Result.Failure("Participant already exists for this event");
            }

            await _context.Participants.AddAsync(participant);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error occurred while adding participant");
            return Result.Failure($"Failed to add participant: {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding participant");
            return Result.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(Participant participant)
    {
        try
        {
            _context.Participants.Update(participant);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating participant");
            return Result.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task<Result> RemoveAsync(Guid eventId, Guid userId)
    {
        try
        {
            var participant = await _context.Participants
                .Include(p => p.Event)
                .FirstOrDefaultAsync(p => p.EventId == eventId && p.UserId == userId);
                
            if (participant == null)
            {
                return Result.Failure("Participant not found");
            }

            // Check if user is the organizer
            if (participant.Event.OrganizerId == userId)
            {
                return Result.Failure("Cannot remove the organizer from the event");
            }

            _context.Participants.Remove(participant);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while removing participant for event {EventId} and user {UserId}", eventId, userId);
            return Result.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task<IReadOnlyList<Participant>> GetByUserIdAsync(Guid userId, ParticipantStatus? status = null)
    {
        try
        {
            var query = _context.Participants
                .Include(p => p.Event)
                    .ThenInclude(e => e.Organizer)
                .Include(p => p.User)
                .Where(p => p.UserId == userId);
                
            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status);
            }
            
            return await query
                .OrderBy(p => EF.Property<DateTime>(p.Event, "TimeRange_Start"))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting participations for user {UserId}", userId);
            throw;
        }
    }
}