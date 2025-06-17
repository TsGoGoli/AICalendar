using AICalendar.Application.DTOs;
using AICalendar.Domain.Common;

namespace AICalendar.Application.Common.Services;

/// <summary>
/// Interface for authentication service
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user and returns a token if successful
    /// </summary>
    /// <param name="loginDto">The login credentials</param>
    /// <returns>Authentication result with token if successful</returns>
    Task<Result<AuthResponseDto>> LoginAsync(LoginDto loginDto);
    
    /// <summary>
    /// Registers a new user
    /// </summary>
    /// <param name="registerDto">The registration information</param>
    /// <returns>Authentication result with token if successful</returns>
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto registerDto);
}