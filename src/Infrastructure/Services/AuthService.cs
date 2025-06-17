using AICalendar.Application.Common.Services;
using AICalendar.Application.DTOs;
using AICalendar.Domain.Common;
using AICalendar.Domain.Entities;
using AICalendar.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace AICalendar.Infrastructure.Services;

/// <summary>
/// Implementation of authentication service
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IJwtService jwtService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto loginDto)
    {
        try
        {
            // Validate input
            if (string.IsNullOrEmpty(loginDto.Email) || string.IsNullOrEmpty(loginDto.Password))
                return Result.Failure<AuthResponseDto>("Email and password are required.");

            // Find user by email
            var user = await _userRepository.GetByEmailAsync(loginDto.Email);
            if (user == null)
                return Result.Failure<AuthResponseDto>("Invalid email or password.");

            // Verify password
            if (!user.VerifyPassword(loginDto.Password))
                return Result.Failure<AuthResponseDto>("Invalid email or password.");

            // Generate token
            var (token, expiration) = _jwtService.GenerateToken(user);

            // Create response
            var response = new AuthResponseDto
            {
                IsSuccess = true,
                Token = token,
                ExpiresAt = expiration,
                User = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Username = user.Username,
                    CreatedAt = user.CreatedAt,
                    LastModifiedAt = user.LastModifiedAt
                }
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user login");
            return Result.Failure<AuthResponseDto>("An error occurred during login.");
        }
    }

    public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            // Validate input
            if (string.IsNullOrEmpty(registerDto.Email) || string.IsNullOrEmpty(registerDto.Name))
                return Result.Failure<AuthResponseDto>("Name and email are required.");

            if (string.IsNullOrEmpty(registerDto.Password))
                return Result.Failure<AuthResponseDto>("Password is required.");

            if (registerDto.Password != registerDto.ConfirmPassword)
                return Result.Failure<AuthResponseDto>("Passwords do not match.");

            // Check if user already exists
            var existingUser = await _userRepository.GetByEmailAsync(registerDto.Email);
            if (existingUser != null)
                return Result.Failure<AuthResponseDto>($"User with email {registerDto.Email} already exists.");

            // Create new user
            var user = new User(registerDto.Name, registerDto.Email, registerDto.Username);
            user.SetPassword(registerDto.Password);

            // Save user
            var result = await _userRepository.CreateAsync(user);
            if (result.IsFailure)
                return Result.Failure<AuthResponseDto>(result.Error);

            // Generate token
            var (token, expiration) = _jwtService.GenerateToken(user);

            // Create response
            var response = new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Registration successful",
                Token = token,
                ExpiresAt = expiration,
                User = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Username = user.Username,
                    CreatedAt = user.CreatedAt,
                    LastModifiedAt = user.LastModifiedAt
                }
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            return Result.Failure<AuthResponseDto>("An error occurred during registration.");
        }
    }
}