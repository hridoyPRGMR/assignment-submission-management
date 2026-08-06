using FluentValidation;

namespace AssignmentSystem.Application.Features.Subjects;

public class UpdateSubjectInputValidator : AbstractValidator<UpdateSubjectInput>
{
    public UpdateSubjectInputValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}
