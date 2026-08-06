using FluentValidation;

namespace AssignmentSystem.Application.Features.StudentEnrollments;

public class EnrollStudentInputValidator : AbstractValidator<EnrollStudentInput>
{
    public EnrollStudentInputValidator()
    {
        RuleFor(x => x.StudentUserId).NotEmpty();
        RuleFor(x => x.ClassId).NotEmpty();
    }
}
