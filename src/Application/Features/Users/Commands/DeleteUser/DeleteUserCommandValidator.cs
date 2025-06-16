using FluentValidation;

namespace AICalendar.Application.Features.Users.Commands.DeleteUser;

/// <summary>
/// Validator for DeleteUserCommand
/// </summary>
public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(u => u.Id)
            .NotEmpty().WithMessage("User ID is required");
    }
}