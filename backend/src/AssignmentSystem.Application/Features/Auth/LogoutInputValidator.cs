using FluentValidation;

namespace AssignmentSystem.Application.Features.Auth;

public class LogoutInputValidator : AbstractValidator<LogoutInput>
{
    public LogoutInputValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
