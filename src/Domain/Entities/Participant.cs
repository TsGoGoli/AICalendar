using AICalendar.Domain.Common;
using AICalendar.Domain.Enums;

namespace AICalendar.Domain.Entities;

/// <summary>
/// Represents a participant in an event (joining User and Event entities)
/// </summary>
public class Participant : BaseEntity
{
    public Guid EventId { get; private set; }
    public Guid UserId { get; private set; }
    public ParticipantStatus Status { get; private set; }
    public string? Note { get; private set; }

    // Navigation properties for EF Core
    public virtual Event Event { get; private set; } = null!;
    public virtual User User { get; private set; } = null!;

    // Required by EF Core
    protected Participant() : base() { }

    public Participant(Event @event, User user, ParticipantStatus status = ParticipantStatus.Pending, string? note = null) : base()
    {
        Event = @event ?? throw new ArgumentNullException(nameof(@event));
        EventId = @event.Id;
        
        User = user ?? throw new ArgumentNullException(nameof(user));
        UserId = user.Id;
        
        Status = status;
        Note = note;
    }

    public void UpdateStatus(ParticipantStatus status, string? note = null)
    {
        Status = status;
        if (note != null)
        {
            Note = note;
        }
        SetLastModified();
    }
}