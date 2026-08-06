using FluentValidation;

namespace AssignmentSystem.Application.Features.Classes;

public class CreateClassInputValidator : AbstractValidator<CreateClassInput>
{
    public CreateClassInputValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}
