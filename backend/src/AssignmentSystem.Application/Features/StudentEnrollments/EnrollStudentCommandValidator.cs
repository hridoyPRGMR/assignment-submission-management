using FluentValidation;

namespace AssignmentSystem.Application.Features.StudentEnrollments;

public class EnrollStudentCommandValidator : AbstractValidator<EnrollStudentCommand>
{
    public EnrollStudentCommandValidator()
    {
        RuleFor(x => x.StudentUserId).NotEmpty();
        RuleFor(x => x.ClassId).NotEmpty();
    }
}
