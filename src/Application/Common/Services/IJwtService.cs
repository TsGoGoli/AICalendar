using AICalendar.Domain.Entities;

namespace AICalendar.Application.Common.Services;

/// <summary>
/// Interface for JWT token service
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generates a JWT token for a user
    /// </summary>
    /// <param name="user">The user to generate a token for</param>
    /// <returns>JWT token and expiration date</returns>
    (string Token, DateTime Expiration) GenerateToken(User user);
}