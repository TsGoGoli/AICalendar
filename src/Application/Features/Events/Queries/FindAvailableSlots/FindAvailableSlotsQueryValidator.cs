using FluentValidation;

namespace AICalendar.Application.Features.Events.Queries.FindAvailableSlots;

/// <summary>
/// Validator for FindAvailableSlotsQuery
/// </summary>
public class FindAvailableSlotsQueryValidator : AbstractValidator<FindAvailableSlotsQuery>
{
    public FindAvailableSlotsQueryValidator()
    {
        RuleFor(q => q.UserIds)
            .NotEmpty().WithMessage("At least one user ID must be specified");
            
        RuleFor(q => q.Start)
            .NotEmpty().WithMessage("Start time is required")
            .LessThan(q => q.End).WithMessage("Start time must be before end time");
            
        RuleFor(q => q.End)
            .NotEmpty().WithMessage("End time is required");
            
        RuleFor(q => q.Duration)
            .NotEmpty().WithMessage("Duration is required")
            .GreaterThan(TimeSpan.Zero).WithMessage("Duration must be greater than zero");
            
        RuleFor(q => q.MaxResults)
            .GreaterThanOrEqualTo(1).WithMessage("Maximum results must be at least 1");
    }
}