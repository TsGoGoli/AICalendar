using AICalendar.Application.Common.Commands;
using AICalendar.Application.DTOs;
using AICalendar.Domain.Enums;

namespace AICalendar.Application.Features.Participants.Commands.UpdateParticipantStatus;

/// <summary>
/// Command to update a participant's status for an event
/// </summary>
public class UpdateParticipantStatusCommand : ICommand<ParticipantDto>
{
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public ParticipantStatus Status { get; set; }
    public string? Note { get; set; }
    
    public UpdateParticipantStatusCommand()
    {
    }
    
    public UpdateParticipantStatusCommand(Guid eventId, Guid userId, UpdateParticipantStatusDto dto)
    {
        EventId = eventId;
        UserId = userId;
        Status = dto.Status;
        Note = dto.Note;
    }
}