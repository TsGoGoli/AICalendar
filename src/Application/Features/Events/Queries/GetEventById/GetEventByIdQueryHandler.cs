using AICalendar.Application.DTOs;
using AICalendar.Domain.Common;
using AICalendar.Domain.Repositories;
using MediatR;

namespace AICalendar.Application.Features.Events.Queries.GetEventById;

/// <summary>
/// Handler for GetEventByIdQuery
/// </summary>
public class GetEventByIdQueryHandler : IRequestHandler<GetEventByIdQuery, Result<EventDto>>
{
    private readonly IEventRepository _eventRepository;

    public GetEventByIdQueryHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<Result<EventDto>> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var @event = await _eventRepository.GetByIdAsync(request.Id);
            if (@event == null)
            {
                return Result.Failure<EventDto>($"Event with ID {request.Id} not found");
            }

            var eventDto = new EventDto
            {
                Id = @event.Id,
                Title = @event.Title,
                Description = @event.Description,
                Start = @event.TimeRange.Start,
                End = @event.TimeRange.End,
                Status = @event.Status,
                OrganizerId = @event.OrganizerId,
                OrganizerName = @event.Organizer.Name,
                CreatedAt = @event.CreatedAt,
                LastModifiedAt = @event.LastModifiedAt,
                Participants = @event.Participants.Select(p => new ParticipantDto
                {
                    UserId = p.UserId,
                    UserName = p.User.Name,
                    Email = p.User.Email,
                    Status = p.Status,
                    Note = p.Note
                }).ToList()
            };

            return Result.Success(eventDto);
        }
        catch (Exception ex)
        {
            return Result.Failure<EventDto>($"Failed to retrieve event: {ex.Message}");
        }
    }
}