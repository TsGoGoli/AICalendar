using AICalendar.Application.Common.Queries;
using AICalendar.Application.DTOs;
using AICalendar.Domain.Common;

namespace AICalendar.Application.Features.Users.Queries.GetUsers;

/// <summary>
/// Query to get a list of users with optional filtering
/// </summary>
public class GetUsersQuery : IQuery<IReadOnlyList<UserDto>>
{
    public string? NameFilter { get; set; }
    public string? EmailFilter { get; set; }
}