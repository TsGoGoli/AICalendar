using FluentValidation;

namespace AICalendar.Application.Features.Events.Commands.DeleteEvent;

/// <summary>
/// Validator for DeleteEventCommand
/// </summary>
public class DeleteEventCommandValidator : AbstractValidator<DeleteEventCommand>
{
    public DeleteEventCommandValidator()
    {
        RuleFor(e => e.Id)
            .NotEmpty().WithMessage("Event ID is required");
    }
}