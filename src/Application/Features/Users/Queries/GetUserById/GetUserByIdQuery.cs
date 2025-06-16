using AICalendar.Application.Common.Queries;
using AICalendar.Application.DTOs;

namespace AICalendar.Application.Features.Users.Queries.GetUserById;

/// <summary>
/// Query to get a user by its ID
/// </summary>
public class GetUserByIdQuery : IQuery<UserDto>
{
    public Guid Id { get; set; }
    
    public GetUserByIdQuery(Guid id)
    {
        Id = id;
    }
}