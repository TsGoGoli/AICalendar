using AICalendar.Application.Common.Commands;
using AICalendar.Application.DTOs;

namespace AICalendar.Application.Features.Users.Commands.CreateUser;

/// <summary>
/// Command to create a new user
/// </summary>
public class CreateUserCommand : ICommand<UserDto>
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Username { get; set; }
    
    public CreateUserCommand()
    {
    }
    
    public CreateUserCommand(CreateUserDto dto)
    {
        Name = dto.Name;
        Email = dto.Email;
        Username = dto.Username;
    }
}