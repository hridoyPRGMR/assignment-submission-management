using FluentValidation;

namespace AssignmentSystem.Application.Features.ClassSubjects;

public class AssignSubjectToClassInputValidator : AbstractValidator<AssignSubjectToClassInput>
{
    public AssignSubjectToClassInputValidator()
    {
        RuleFor(x => x.ClassId).NotEmpty();
        RuleFor(x => x.SubjectId).NotEmpty();
    }
}
