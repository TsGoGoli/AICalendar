using AICalendar.Application.DTOs;
using AICalendar.Domain.Common;
using AICalendar.Domain.Repositories;
using MediatR;

namespace AICalendar.Application.Features.Participants.Commands.UpdateParticipantStatus;

/// <summary>
/// Handler for UpdateParticipantStatusCommand
/// </summary>
public class UpdateParticipantStatusCommandHandler : IRequestHandler<UpdateParticipantStatusCommand, Result<ParticipantDto>>
{
    private readonly IParticipantRepository _participantRepository;

    public UpdateParticipantStatusCommandHandler(IParticipantRepository participantRepository)
    {
        _participantRepository = participantRepository;
    }

    public async Task<Result<ParticipantDto>> Handle(UpdateParticipantStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if participant exists
            var participant = await _participantRepository.GetByEventIdAndUserIdAsync(request.EventId, request.UserId);
            if (participant == null)
            {
                return Result.Failure<ParticipantDto>($"Participant not found for event ID {request.EventId} and user ID {request.UserId}");
            }

            // Update participant status
            participant.UpdateStatus(request.Status, request.Note);

            // Save changes
            var result = await _participantRepository.UpdateAsync(participant);
            if (result.IsFailure)
            {
                return Result.Failure<ParticipantDto>(result.Error);
            }

            // Return updated participant DTO
            var participantDto = new ParticipantDto
            {
                UserId = participant.UserId,
                UserName = participant.User.Name,
                Email = participant.User.Email,
                Status = participant.Status,
                Note = participant.Note
            };

            return Result.Success(participantDto);
        }
        catch (Exception ex)
        {
            return Result.Failure<ParticipantDto>($"Failed to update participant status: {ex.Message}");
        }
    }
}