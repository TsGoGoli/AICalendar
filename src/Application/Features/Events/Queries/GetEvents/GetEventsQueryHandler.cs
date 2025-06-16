using AICalendar.Application.DTOs;
using AICalendar.Domain.Common;
using AICalendar.Domain.Repositories;
using MediatR;

namespace AICalendar.Application.Features.Events.Queries.GetEvents;

/// <summary>
/// Handler for GetEventsQuery
/// </summary>
public class GetEventsQueryHandler : IRequestHandler<GetEventsQuery, Result<IReadOnlyList<EventDto>>>
{
    private readonly IEventRepository _eventRepository;

    public GetEventsQueryHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<Result<IReadOnlyList<EventDto>>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var events = await _eventRepository.GetAllAsync(
                timeRange: request.GetTimeRangeFilter(), 
                status: request.Status);

            var eventDtos = events.Select(e => new EventDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Start = e.TimeRange.Start,
                End = e.TimeRange.End,
                Status = e.Status,
                OrganizerId = e.OrganizerId,
                OrganizerName = e.Organizer.Name,
                CreatedAt = e.CreatedAt,
                LastModifiedAt = e.LastModifiedAt,
                Participants = e.Participants.Select(p => new ParticipantDto
                {
                    UserId = p.UserId,
                    UserName = p.User.Name,
                    Email = p.User.Email,
                    Status = p.Status,
                    Note = p.Note
                }).ToList()
            }).ToList();

            return Result.Success<IReadOnlyList<EventDto>>(eventDtos);
        }
        catch (Exception ex)
        {
            return Result.Failure<IReadOnlyList<EventDto>>($"Failed to retrieve events: {ex.Message}");
        }
    }
}