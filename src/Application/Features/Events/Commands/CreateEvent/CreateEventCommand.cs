using AICalendar.Application.Common.Commands;
using AICalendar.Application.DTOs;

namespace AICalendar.Application.Features.Events.Commands.CreateEvent;

/// <summary>
/// Command to create a new event
/// </summary>
public class CreateEventCommand : ICommand<EventDto>
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public Guid OrganizerId { get; set; }
    public List<Guid>? ParticipantIds { get; set; }
    
    public CreateEventCommand()
    {
    }
    
    public CreateEventCommand(CreateEventDto dto, Guid organizerId)
    {
        Title = dto.Title;
        Description = dto.Description;
        Start = dto.Start;
        End = dto.End;
        OrganizerId = organizerId;
        ParticipantIds = dto.ParticipantIds;
    }
}