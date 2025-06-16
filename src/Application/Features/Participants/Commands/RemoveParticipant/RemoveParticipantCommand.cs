using AICalendar.Application.Common.Commands;

namespace AICalendar.Application.Features.Participants.Commands.RemoveParticipant;

/// <summary>
/// Command to remove a participant from an event
/// </summary>
public class RemoveParticipantCommand : ICommand
{
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    
    public RemoveParticipantCommand()
    {
    }
    
    public RemoveParticipantCommand(Guid eventId, Guid userId)
    {
        EventId = eventId;
        UserId = userId;
    }
}