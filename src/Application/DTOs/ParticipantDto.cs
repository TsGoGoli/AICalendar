using AICalendar.Domain.Enums;

namespace AICalendar.Application.DTOs;

/// <summary>
/// Data transfer object for Participant entity
/// </summary>
public class ParticipantDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public ParticipantStatus Status { get; set; }
    public string? Note { get; set; }
}

/// <summary>
/// Data transfer object for adding a participant to an event
/// </summary>
public class AddParticipantDto
{
    public Guid UserId { get; set; }
    public ParticipantStatus? Status { get; set; }
    public string? Note { get; set; }
}

/// <summary>
/// Data transfer object for updating a participant's status
/// </summary>
public class UpdateParticipantStatusDto
{
    public ParticipantStatus Status { get; set; }
    public string? Note { get; set; }
}