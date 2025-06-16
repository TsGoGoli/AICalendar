namespace AICalendar.Domain.Enums;

/// <summary>
/// Represents the response status of a participant for an event
/// </summary>
public enum ParticipantStatus
{
    Pending = 0,
    Accepted = 1,
    Declined = 2,
    Tentative = 3
}