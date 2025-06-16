using AICalendar.Application.Common.Commands;
using AICalendar.Application.DTOs;
using AICalendar.Domain.Enums;

namespace AICalendar.Application.Features.Events.Commands.UpdateEvent;

/// <summary>
/// Command to update an existing event
/// </summary>
public class UpdateEventCommand : ICommand
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public EventStatus? Status { get; set; }
    
    public UpdateEventCommand()
    {
    }
    
    public UpdateEventCommand(Guid id, UpdateEventDto dto)
    {
        Id = id;
        Title = dto.Title;
        Description = dto.Description;
        Start = dto.Start;
        End = dto.End;
        Status = dto.Status;
    }
}