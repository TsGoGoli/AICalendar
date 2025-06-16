using AICalendar.Application.Common.Queries;
using AICalendar.Application.DTOs;

namespace AICalendar.Application.Features.Participants.Queries.GetEventParticipants;

/// <summary>
/// Query to get all participants for a specific event
/// </summary>
public class GetEventParticipantsQuery : IQuery<IReadOnlyList<ParticipantDto>>
{
    public Guid EventId { get; set; }
    
    public GetEventParticipantsQuery(Guid eventId)
    {
        EventId = eventId;
    }
}