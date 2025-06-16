using FluentValidation;

namespace AICalendar.Application.Features.Participants.Commands.RemoveParticipant;

/// <summary>
/// Validator for RemoveParticipantCommand
/// </summary>
public class RemoveParticipantCommandValidator : AbstractValidator<RemoveParticipantCommand>
{
    public RemoveParticipantCommandValidator()
    {
        RuleFor(p => p.EventId)
            .NotEmpty().WithMessage("Event ID is required");
            
        RuleFor(p => p.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}