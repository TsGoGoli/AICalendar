using FluentValidation;

namespace AICalendar.Application.Features.Participants.Commands.UpdateParticipantStatus;

/// <summary>
/// Validator for UpdateParticipantStatusCommand
/// </summary>
public class UpdateParticipantStatusCommandValidator : AbstractValidator<UpdateParticipantStatusCommand>
{
    public UpdateParticipantStatusCommandValidator()
    {
        RuleFor(p => p.EventId)
            .NotEmpty().WithMessage("Event ID is required");
            
        RuleFor(p => p.UserId)
            .NotEmpty().WithMessage("User ID is required");
            
        RuleFor(p => p.Status)
            .IsInEnum().WithMessage("Invalid participant status");
            
        RuleFor(p => p.Note)
            .MaximumLength(500).WithMessage("Note cannot exceed 500 characters")
            .When(p => !string.IsNullOrEmpty(p.Note));
    }
}