using AICalendar.Domain.Common;
using AICalendar.Domain.Repositories;
using MediatR;

namespace AICalendar.Application.Features.Users.Commands.UpdateUser;

/// <summary>
/// Handler for UpdateUserCommand
/// </summary>
public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find the user to update
            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user == null)
            {
                return Result.Failure($"User with ID {request.Id} not found");
            }

            // Check if email is being changed and if it's already in use
            if (user.Email != request.Email)
            {
                var existingUser = await _userRepository.GetByEmailAsync(request.Email);
                if (existingUser != null && existingUser.Id != request.Id)
                {
                    return Result.Failure($"Email {request.Email} is already in use by another user");
                }
            }

            // Update user properties
            user.Update(request.Name, request.Email, request.Username);

            // Save changes
            return await _userRepository.UpdateAsync(user);
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to update user: {ex.Message}");
        }
    }
}