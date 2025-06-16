using AICalendar.Application.DTOs;
using AICalendar.Domain.Common;
using AICalendar.Domain.Entities;
using AICalendar.Domain.Repositories;
using AICalendar.Domain.ValueObjects;
using MediatR;

namespace AICalendar.Application.Features.Events.Commands.CreateEvent;

/// <summary>
/// Handler for CreateEventCommand
/// </summary>
public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, Result<EventDto>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IUserRepository _userRepository;

    public CreateEventCommandHandler(IEventRepository eventRepository, IUserRepository userRepository)
    {
        _eventRepository = eventRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<EventDto>> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the organizer
            var organizer = await _userRepository.GetByIdAsync(request.OrganizerId);
            if (organizer == null)
            {
                return Result.Failure<EventDto>($"Organizer with ID {request.OrganizerId} not found");
            }

            // Create the event with time range
            var timeRange = new DateTimeRange(request.Start, request.End);
            var @event = new Event(request.Title, timeRange, organizer, request.Description);

            // Add additional participants if specified
            if (request.ParticipantIds != null && request.ParticipantIds.Any())
            {
                foreach (var participantId in request.ParticipantIds)
                {
                    // Skip the organizer as they're already added automatically
                    if (participantId == request.OrganizerId)
                        continue;

                    var user = await _userRepository.GetByIdAsync(participantId);
                    if (user != null)
                    {
                        @event.AddParticipant(user);
                    }
                }
            }

            // Save the event
            var result = await _eventRepository.CreateAsync(@event);
            if (result.IsFailure)
            {
                return Result.Failure<EventDto>(result.Error);
            }

            // Create the response DTO
            var eventDto = new EventDto
            {
                Id = @event.Id,
                Title = @event.Title,
                Description = @event.Description,
                Start = @event.TimeRange.Start,
                End = @event.TimeRange.End,
                Status = @event.Status,
                OrganizerId = @event.OrganizerId,
                OrganizerName = organizer.Name,
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
            return Result.Failure<EventDto>($"Failed to create event: {ex.Message}");
        }
    }
}