using AICalendar.Domain.Common;
using AICalendar.Domain.Entities;
using AICalendar.Domain.Repositories;
using AICalendar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AICalendar.Infrastructure.Repositories;

/// <summary>
/// Implementation of IUserRepository using Entity Framework Core
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(ApplicationDbContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(string? nameFilter = null, string? emailFilter = null)
    {
        try
        {
            var query = _context.Users.AsQueryable();
            
            if (!string.IsNullOrWhiteSpace(nameFilter))
            {
                query = query.Where(u => u.Name.Contains(nameFilter));
            }
            
            if (!string.IsNullOrWhiteSpace(emailFilter))
            {
                query = query.Where(u => u.Email.Contains(emailFilter));
            }
            
            return await query
                .OrderBy(u => u.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all users");
            throw;
        }
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting user by ID {UserId}", id);
            throw;
        }
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        try
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting user by email {Email}", email);
            throw;
        }
    }

    public async Task<Result> CreateAsync(User user)
    {
        try
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error occurred while creating user");
            return Result.Failure($"Failed to create user: {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating user");
            return Result.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(User user)
    {
        try
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error occurred while updating user {UserId}", user.Id);
            return Result.Failure($"Failed to update user: {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating user {UserId}", user.Id);
            return Result.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return Result.Failure($"User with ID {id} not found");
            }

            // Check if user is an organizer of any events
            var isOrganizer = await _context.Events
                .AnyAsync(e => e.OrganizerId == id);
                
            if (isOrganizer)
            {
                return Result.Failure("Cannot delete user who is an organizer of one or more events");
            }

            // Check if user participates in any events and remove them
            var participations = await _context.Participants
                .Where(p => p.UserId == id)
                .ToListAsync();
                
            if (participations.Any())
            {
                _context.Participants.RemoveRange(participations);
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting user {UserId}", id);
            return Result.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }
}