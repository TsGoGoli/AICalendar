using AICalendar.Domain.Enums;

namespace AICalendar.Application.DTOs;

/// <summary>
/// Data transfer object for Event entity
/// </summary>
public class EventDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public EventStatus Status { get; set; }
    public Guid OrganizerId { get; set; }
    public string OrganizerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public IList<ParticipantDto>? Participants { get; set; }
}

/// <summary>
/// Data transfer object for creating an event
/// </summary>
public class CreateEventDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public List<Guid>? ParticipantIds { get; set; }
}

/// <summary>
/// Data transfer object for updating an event
/// </summary>
public class UpdateEventDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public EventStatus? Status { get; set; }
}