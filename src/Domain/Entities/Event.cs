using AICalendar.Domain.Common;
using AICalendar.Domain.Enums;
using AICalendar.Domain.ValueObjects;

namespace AICalendar.Domain.Entities;

/// <summary>
/// Represents a calendar event
/// </summary>
public class Event : BaseEntity
{
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public DateTimeRange TimeRange { get; private set; }
    public EventStatus Status { get; private set; }
    public Guid OrganizerId { get; private set; }

    // Navigation properties for EF Core
    public virtual User Organizer { get; private set; } = null!;
    public virtual ICollection<Participant> Participants { get; private set; } = new List<Participant>();

    // Required by EF Core
    protected Event() : base() { }

    public Event(string title, DateTimeRange timeRange, User organizer, string? description = null) : base()
    {
        Title = !string.IsNullOrWhiteSpace(title) ? title : throw new ArgumentException("Title cannot be empty", nameof(title));
        Description = description;
        TimeRange = timeRange ?? throw new ArgumentNullException(nameof(timeRange));
        Status = EventStatus.Scheduled;
        
        Organizer = organizer ?? throw new ArgumentNullException(nameof(organizer));
        OrganizerId = organizer.Id;
        
        // Add organizer as a participant automatically
        AddParticipant(organizer, ParticipantStatus.Accepted);
    }

    public Event(Guid id, string title, DateTimeRange timeRange, User organizer, string? description = null) : base(id)
    {
        Title = !string.IsNullOrWhiteSpace(title) ? title : throw new ArgumentException("Title cannot be empty", nameof(title));
        Description = description;
        TimeRange = timeRange ?? throw new ArgumentNullException(nameof(timeRange));
        Status = EventStatus.Scheduled;
        
        Organizer = organizer ?? throw new ArgumentNullException(nameof(organizer));
        OrganizerId = organizer.Id;
        
        // Add organizer as a participant automatically
        AddParticipant(organizer, ParticipantStatus.Accepted);
    }

    public void Update(string title, DateTimeRange timeRange, string? description = null)
    {
        Title = !string.IsNullOrWhiteSpace(title) ? title : throw new ArgumentException("Title cannot be empty", nameof(title));
        TimeRange = timeRange ?? throw new ArgumentNullException(nameof(timeRange));
        Description = description;
        SetLastModified();
    }

    public void Cancel()
    {
        if (Status == EventStatus.Cancelled)
            throw new InvalidOperationException("Event is already cancelled");
            
        Status = EventStatus.Cancelled;
        SetLastModified();
    }

    public void Complete()
    {
        if (Status == EventStatus.Cancelled)
            throw new InvalidOperationException("Cannot complete a cancelled event");
            
        if (Status == EventStatus.Completed)
            throw new InvalidOperationException("Event is already completed");
            
        Status = EventStatus.Completed;
        SetLastModified();
    }

    public Participant AddParticipant(User user, ParticipantStatus status = ParticipantStatus.Pending)
    {
        if (Status == EventStatus.Cancelled)
            throw new InvalidOperationException("Cannot add participant to a cancelled event");

        // Check if user is already a participant
        var existingParticipant = Participants.FirstOrDefault(p => p.UserId == user.Id);
        if (existingParticipant != null)
            return existingParticipant;

        var participant = new Participant(this, user, status);
        Participants.Add(participant);
        SetLastModified();
        
        return participant;
    }

    public bool RemoveParticipant(Guid userId)
    {
        if (userId == OrganizerId)
            throw new InvalidOperationException("Cannot remove the organizer from the event");
            
        var participant = Participants.FirstOrDefault(p => p.UserId == userId);
        if (participant == null)
            return false;
            
        Participants.Remove(participant);
        SetLastModified();
        return true;
    }
}