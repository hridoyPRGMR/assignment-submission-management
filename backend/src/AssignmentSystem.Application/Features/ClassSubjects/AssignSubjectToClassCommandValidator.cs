using FluentValidation;

namespace AssignmentSystem.Application.Features.ClassSubjects;

public class AssignSubjectToClassCommandValidator : AbstractValidator<AssignSubjectToClassCommand>
{
    public AssignSubjectToClassCommandValidator()
    {
        RuleFor(x => x.ClassId).NotEmpty();
        RuleFor(x => x.SubjectId).NotEmpty();
    }
}
