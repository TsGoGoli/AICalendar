using AICalendar.Application.Common.Commands;

namespace AICalendar.Application.Features.Users.Commands.DeleteUser;

/// <summary>
/// Command to delete a user
/// </summary>
public class DeleteUserCommand : ICommand
{
    public Guid Id { get; set; }
    
    public DeleteUserCommand()
    {
    }
    
    public DeleteUserCommand(Guid id)
    {
        Id = id;
    }
}