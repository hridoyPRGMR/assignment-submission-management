using FluentValidation;

namespace AssignmentSystem.Application.Features.TeacherAssignments;

public class AssignTeacherInputValidator : AbstractValidator<AssignTeacherInput>
{
    public AssignTeacherInputValidator()
    {
        RuleFor(x => x.TeacherUserId).NotEmpty();
        RuleFor(x => x.ClassId).NotEmpty();
        RuleFor(x => x.SubjectId).NotEmpty();
    }
}
