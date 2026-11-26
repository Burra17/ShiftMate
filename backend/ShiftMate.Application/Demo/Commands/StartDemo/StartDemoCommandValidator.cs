using FluentValidation;

namespace ShiftMate.Application.Demo.Commands.StartDemo;

// Demon finns bara som chef eller anställd – aldrig SuperAdmin.
public class StartDemoCommandValidator : AbstractValidator<StartDemoCommand>
{
    public StartDemoCommandValidator()
    {
        RuleFor(x => x.Role)
            .Must(role => role is "Manager" or "Employee")
            .WithMessage("Rollen måste vara Manager eller Employee.");
    }
}
