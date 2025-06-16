using AICalendar.Application.DTOs;
using AICalendar.Domain.Common;
using AICalendar.Domain.Repositories;
using MediatR;

namespace AICalendar.Application.Features.Participants.Queries.GetEventParticipants;

/// <summary>
/// Handler for GetEventParticipantsQuery
/// </summary>
public class GetEventParticipantsQueryHandler : IRequestHandler<GetEventParticipantsQuery, Result<IReadOnlyList<ParticipantDto>>>
{
    private readonly IParticipantRepository _participantRepository;
    private readonly IEventRepository _eventRepository;

    public GetEventParticipantsQueryHandler(
        IParticipantRepository participantRepository,
        IEventRepository eventRepository)
    {
        _participantRepository = participantRepository;
        _eventRepository = eventRepository;
    }

    public async Task<Result<IReadOnlyList<ParticipantDto>>> Handle(GetEventParticipantsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify the event exists
            var @event = await _eventRepository.GetByIdAsync(request.EventId);
            if (@event == null)
            {
                return Result.Failure<IReadOnlyList<ParticipantDto>>($"Event with ID {request.EventId} not found");
            }

            // Get participants
            var participants = await _participantRepository.GetByEventIdAsync(request.EventId);
            
            // Map to DTOs
            var participantDtos = participants.Select(p => new ParticipantDto
            {
                UserId = p.UserId,
                UserName = p.User.Name,
                Email = p.User.Email,
                Status = p.Status,
                Note = p.Note
            }).ToList();

            return Result.Success<IReadOnlyList<ParticipantDto>>(participantDtos);
        }
        catch (Exception ex)
        {
            return Result.Failure<IReadOnlyList<ParticipantDto>>($"Failed to retrieve event participants: {ex.Message}");
        }
    }
}