using AICalendar.Application.DTOs;
using AICalendar.Domain.Common;
using AICalendar.Domain.Entities;
using AICalendar.Domain.Repositories;
using MediatR;

namespace AICalendar.Application.Features.Users.Commands.CreateUser;

/// <summary>
/// Handler for CreateUserCommand
/// </summary>
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public CreateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if a user with the same email already exists
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return Result.Failure<UserDto>($"User with email {request.Email} already exists");
            }

            // Create new user
            var user = new User(request.Name, request.Email, request.Username);

            // Save to repository
            var result = await _userRepository.CreateAsync(user);
            if (result.IsFailure)
            {
                return Result.Failure<UserDto>(result.Error);
            }

            // Return DTO
            var userDto = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Username = user.Username,
                CreatedAt = user.CreatedAt,
                LastModifiedAt = user.LastModifiedAt
            };

            return Result.Success(userDto);
        }
        catch (Exception ex)
        {
            return Result.Failure<UserDto>($"Failed to create user: {ex.Message}");
        }
    }
}