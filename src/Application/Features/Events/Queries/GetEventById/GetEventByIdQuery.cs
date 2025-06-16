using AICalendar.Application.Common.Queries;
using AICalendar.Application.DTOs;

namespace AICalendar.Application.Features.Events.Queries.GetEventById;

/// <summary>
/// Query to get an event by its ID
/// </summary>
public class GetEventByIdQuery : IQuery<EventDto>
{
    public Guid Id { get; set; }
    
    public GetEventByIdQuery(Guid id)
    {
        Id = id;
    }
}