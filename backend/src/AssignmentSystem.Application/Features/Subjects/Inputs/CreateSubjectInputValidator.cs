using FluentValidation;

namespace AssignmentSystem.Application.Features.Subjects;

public class CreateSubjectInputValidator : AbstractValidator<CreateSubjectInput>
{
    public CreateSubjectInputValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}
