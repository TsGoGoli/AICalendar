using AICalendar.Application.DTOs;
using AICalendar.Domain.Common;
using AICalendar.Domain.Repositories;
using MediatR;

namespace AICalendar.Application.Features.Users.Queries.GetUsers;

/// <summary>
/// Handler for the GetUsersQuery
/// </summary>
public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<IReadOnlyList<UserDto>>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<IReadOnlyList<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var users = await _userRepository.GetAllAsync(
                nameFilter: request.NameFilter,
                emailFilter: request.EmailFilter);

            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Username = u.Username,
                CreatedAt = u.CreatedAt,
                LastModifiedAt = u.LastModifiedAt
            }).ToList();

            return Result.Success<IReadOnlyList<UserDto>>(userDtos);
        }
        catch (Exception ex)
        {
            return Result.Failure<IReadOnlyList<UserDto>>($"Failed to retrieve users: {ex.Message}");
        }
    }
}