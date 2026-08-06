using FluentValidation;

namespace AssignmentSystem.Application.Features.Classes;

public class UpdateClassInputValidator : AbstractValidator<UpdateClassInput>
{
    public UpdateClassInputValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}
