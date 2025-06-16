using AICalendar.Application.Common.Commands;
using AICalendar.Application.DTOs;
using AICalendar.Domain.Enums;

namespace AICalendar.Application.Features.Participants.Commands.AddParticipant;

/// <summary>
/// Command to add a participant to an event
/// </summary>
public class AddParticipantCommand : ICommand<ParticipantDto>
{
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public ParticipantStatus? Status { get; set; }
    public string? Note { get; set; }
    
    public AddParticipantCommand()
    {
    }
    
    public AddParticipantCommand(Guid eventId, AddParticipantDto dto)
    {
        EventId = eventId;
        UserId = dto.UserId;
        Status = dto.Status;
        Note = dto.Note;
    }
}