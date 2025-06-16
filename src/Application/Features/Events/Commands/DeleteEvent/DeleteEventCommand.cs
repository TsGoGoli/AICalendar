using AICalendar.Application.Common.Commands;

namespace AICalendar.Application.Features.Events.Commands.DeleteEvent;

/// <summary>
/// Command to delete an event
/// </summary>
public class DeleteEventCommand : ICommand
{
    public Guid Id { get; set; }
    
    public DeleteEventCommand()
    {
    }
    
    public DeleteEventCommand(Guid id)
    {
        Id = id;
    }
}