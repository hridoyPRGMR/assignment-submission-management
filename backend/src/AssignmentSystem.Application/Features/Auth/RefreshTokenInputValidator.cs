using FluentValidation;

namespace AssignmentSystem.Application.Features.Auth;

public class RefreshTokenInputValidator : AbstractValidator<RefreshTokenInput>
{
    public RefreshTokenInputValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
