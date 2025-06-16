using AICalendar.Domain.Common;
using AICalendar.Domain.Repositories;
using MediatR;

namespace AICalendar.Application.Features.Participants.Commands.RemoveParticipant;

/// <summary>
/// Handler for RemoveParticipantCommand
/// </summary>
public class RemoveParticipantCommandHandler : IRequestHandler<RemoveParticipantCommand, Result>
{
    private readonly IParticipantRepository _participantRepository;
    private readonly IEventRepository _eventRepository;

    public RemoveParticipantCommandHandler(
        IParticipantRepository participantRepository,
        IEventRepository eventRepository)
    {
        _participantRepository = participantRepository;
        _eventRepository = eventRepository;
    }

    public async Task<Result> Handle(RemoveParticipantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if event exists
            var @event = await _eventRepository.GetByIdAsync(request.EventId);
            if (@event == null)
            {
                return Result.Failure($"Event with ID {request.EventId} not found");
            }

            // Check if the user is the organizer (organizer cannot be removed)
            if (@event.OrganizerId == request.UserId)
            {
                return Result.Failure("Cannot remove the organizer from the event");
            }

            // Check if participant exists
            var participant = await _participantRepository.GetByEventIdAndUserIdAsync(request.EventId, request.UserId);
            if (participant == null)
            {
                return Result.Failure($"Participant not found for event ID {request.EventId} and user ID {request.UserId}");
            }

            // Remove participant
            return await _participantRepository.RemoveAsync(request.EventId, request.UserId);
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to remove participant: {ex.Message}");
        }
    }
}