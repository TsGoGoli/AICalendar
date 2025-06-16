using AICalendar.Application.Common.Commands;
using AICalendar.Application.DTOs;

namespace AICalendar.Application.Features.Users.Commands.UpdateUser;

/// <summary>
/// Command to update an existing user
/// </summary>
public class UpdateUserCommand : ICommand
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Username { get; set; }
    
    public UpdateUserCommand()
    {
    }
    
    public UpdateUserCommand(Guid id, UpdateUserDto dto)
    {
        Id = id;
        Name = dto.Name;
        Email = dto.Email;
        Username = dto.Username;
    }
}