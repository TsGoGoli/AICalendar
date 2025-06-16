using AICalendar.Domain.Common;
using AICalendar.Domain.Entities;
using AICalendar.Domain.Enums;
using AICalendar.Domain.Repositories;
using AICalendar.Domain.ValueObjects;
using AICalendar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AICalendar.Infrastructure.Repositories;

/// <summary>
/// Implementation of IEventRepository using Entity Framework Core
/// </summary>
public class EventRepository : IEventRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EventRepository> _logger;

    public EventRepository(ApplicationDbContext context, ILogger<EventRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Event>> GetAllAsync(DateTimeRange? timeRange = null, EventStatus? status = null)
    {
        try
        {
            var query = _context.Events
                .Include(e => e.Organizer)
                .Include(e => e.Participants)
                    .ThenInclude(p => p.User)
                .AsQueryable();
            
            if (timeRange != null)
            {
                // Filter events where timeRange overlaps with event's timeRange
                // Event's start time is before the end of the requested range AND
                // Event's end time is after the start of the requested range
                query = query.Where(e => 
                    EF.Property<DateTime>(e, "TimeRange_Start") < timeRange.End && 
                    EF.Property<DateTime>(e, "TimeRange_End") > timeRange.Start);
            }
            
            if (status.HasValue)
            {
                query = query.Where(e => e.Status == status);
            }
            
            return await query
                .OrderBy(e => EF.Property<DateTime>(e, "TimeRange_Start"))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting events");
            throw;
        }
    }

    public async Task<Event?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _context.Events
                .Include(e => e.Organizer)
                .Include(e => e.Participants)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(e => e.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting event by ID {EventId}", id);
            throw;
        }
    }

    public async Task<IReadOnlyList<Event>> GetByUserIdAndTimeRangeAsync(
        Guid userId, 
        DateTimeRange timeRange, 
        EventStatus? status = null)
    {
        try
        {
            var query = _context.Events
                .Include(e => e.Organizer)
                .Include(e => e.Participants)
                    .ThenInclude(p => p.User)
                .Where(e => 
                    (e.OrganizerId == userId || e.Participants.Any(p => p.UserId == userId)) &&
                    EF.Property<DateTime>(e, "TimeRange_Start") < timeRange.End && 
                    EF.Property<DateTime>(e, "TimeRange_End") > timeRange.Start);
            
            if (status.HasValue)
            {
                query = query.Where(e => e.Status == status);
            }
            
            return await query
                .OrderBy(e => EF.Property<DateTime>(e, "TimeRange_Start"))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting events for user {UserId}", userId);
            throw;
        }
    }

    public async Task<Result> CreateAsync(Event @event)
    {
        try
        {
            await _context.Events.AddAsync(@event);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error occurred while creating event");
            return Result.Failure($"Failed to create event: {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating event");
            return Result.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(Event @event)
    {
        try
        {
            _context.Events.Update(@event);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error occurred while updating event {EventId}", @event.Id);
            return Result.Failure($"Failed to update event: {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating event {EventId}", @event.Id);
            return Result.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        try
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return Result.Failure($"Event with ID {id} not found");
            }

            // Check for participants and remove them
            var participants = await _context.Participants
                .Where(p => p.EventId == id)
                .ToListAsync();
                
            if (participants.Any())
            {
                _context.Participants.RemoveRange(participants);
            }

            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting event {EventId}", id);
            return Result.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }
}