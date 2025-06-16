using AICalendar.Application.DTOs;
using AICalendar.Domain.Common;
using AICalendar.Domain.Repositories;
using MediatR;

namespace AICalendar.Application.Features.Users.Queries.GetUserById;

/// <summary>
/// Handler for GetUserByIdQuery
/// </summary>
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.Id);

            if (user == null)
            {
                return Result.Failure<UserDto>($"User with ID {request.Id} not found");
            }

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
            return Result.Failure<UserDto>($"Failed to retrieve user: {ex.Message}");
        }
    }
}