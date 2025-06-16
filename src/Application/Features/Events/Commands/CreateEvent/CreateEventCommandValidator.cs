using FluentValidation;

namespace AICalendar.Application.Features.Events.Commands.CreateEvent;

/// <summary>
/// Validator for CreateEventCommand
/// </summary>
public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(e => e.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");
            
        RuleFor(e => e.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters")
            .When(e => !string.IsNullOrEmpty(e.Description));
            
        RuleFor(e => e.Start)
            .NotEmpty().WithMessage("Start time is required")
            .LessThan(e => e.End).WithMessage("Start time must be before end time");
            
        RuleFor(e => e.End)
            .NotEmpty().WithMessage("End time is required");
            
        RuleFor(e => e.OrganizerId)
            .NotEmpty().WithMessage("Organizer is required");
            
        RuleForEach(e => e.ParticipantIds)
            .NotEmpty().WithMessage("Participant ID cannot be empty")
            .When(e => e.ParticipantIds != null && e.ParticipantIds.Any());
    }
}