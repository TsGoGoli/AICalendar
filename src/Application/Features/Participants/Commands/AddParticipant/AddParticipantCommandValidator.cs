using FluentValidation;

namespace AICalendar.Application.Features.Participants.Commands.AddParticipant;

/// <summary>
/// Validator for AddParticipantCommand
/// </summary>
public class AddParticipantCommandValidator : AbstractValidator<AddParticipantCommand>
{
    public AddParticipantCommandValidator()
    {
        RuleFor(p => p.EventId)
            .NotEmpty().WithMessage("Event ID is required");
            
        RuleFor(p => p.UserId)
            .NotEmpty().WithMessage("User ID is required");
            
        RuleFor(p => p.Note)
            .MaximumLength(500).WithMessage("Note cannot exceed 500 characters")
            .When(p => !string.IsNullOrEmpty(p.Note));
    }
}