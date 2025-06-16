using AICalendar.Domain.Common;
using AICalendar.Domain.Entities;

namespace AICalendar.Domain.Repositories;

/// <summary>
/// Repository interface for User entity operations
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets all users with optional filtering
    /// </summary>
    /// <param name="nameFilter">Optional filter for user name</param>
    /// <param name="emailFilter">Optional filter for user email</param>
    /// <returns>A list of users matching the filters</returns>
    Task<IReadOnlyList<User>> GetAllAsync(string? nameFilter = null, string? emailFilter = null);
    
    /// <summary>
    /// Gets a user by ID
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>The user if found, null otherwise</returns>
    Task<User?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Gets a user by email
    /// </summary>
    /// <param name="email">The user email</param>
    /// <returns>The user if found, null otherwise</returns>
    Task<User?> GetByEmailAsync(string email);
    
    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="user">The user to create</param>
    Task<Result> CreateAsync(User user);
    
    /// <summary>
    /// Updates an existing user
    /// </summary>
    /// <param name="user">The user to update</param>
    Task<Result> UpdateAsync(User user);
    
    /// <summary>
    /// Deletes a user by ID
    /// </summary>
    /// <param name="id">The ID of the user to delete</param>
    Task<Result> DeleteAsync(Guid id);
}