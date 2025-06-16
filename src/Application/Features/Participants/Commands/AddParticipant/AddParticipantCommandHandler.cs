using AICalendar.Application.DTOs;
using AICalendar.Domain.Common;
using AICalendar.Domain.Entities;
using AICalendar.Domain.Enums;
using AICalendar.Domain.Repositories;
using MediatR;

namespace AICalendar.Application.Features.Participants.Commands.AddParticipant;

/// <summary>
/// Handler for AddParticipantCommand
/// </summary>
public class AddParticipantCommandHandler : IRequestHandler<AddParticipantCommand, Result<ParticipantDto>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IUserRepository _userRepository;
    private readonly IParticipantRepository _participantRepository;

    public AddParticipantCommandHandler(
        IEventRepository eventRepository,
        IUserRepository userRepository,
        IParticipantRepository participantRepository)
    {
        _eventRepository = eventRepository;
        _userRepository = userRepository;
        _participantRepository = participantRepository;
    }

    public async Task<Result<ParticipantDto>> Handle(AddParticipantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if event exists
            var @event = await _eventRepository.GetByIdAsync(request.EventId);
            if (@event == null)
            {
                return Result.Failure<ParticipantDto>($"Event with ID {request.EventId} not found");
            }

            // Check if user exists
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                return Result.Failure<ParticipantDto>($"User with ID {request.UserId} not found");
            }

            // Check if participant already exists
            var existingParticipant = await _participantRepository.GetByEventIdAndUserIdAsync(request.EventId, request.UserId);
            if (existingParticipant != null)
            {
                return Result.Failure<ParticipantDto>($"User is already a participant in this event");
            }

            // Create participant with specified status or default
            var status = request.Status ?? ParticipantStatus.Pending;
            var participant = new Participant(@event, user, status, request.Note);

            // Save participant
            var result = await _participantRepository.AddAsync(participant);
            if (result.IsFailure)
            {
                return Result.Failure<ParticipantDto>(result.Error);
            }

            // Return DTO
            var participantDto = new ParticipantDto
            {
                UserId = participant.UserId,
                UserName = user.Name,
                Email = user.Email,
                Status = participant.Status,
                Note = participant.Note
            };

            return Result.Success(participantDto);
        }
        catch (Exception ex)
        {
            return Result.Failure<ParticipantDto>($"Failed to add participant: {ex.Message}");
        }
    }
}