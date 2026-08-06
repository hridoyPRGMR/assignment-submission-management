using FluentValidation;

namespace AssignmentSystem.Application.Features.Submissions;

public class GradeSubmissionInputValidator : AbstractValidator<GradeSubmissionInput>
{
    public GradeSubmissionInputValidator()
    {
        RuleFor(x => x.Marks).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Feedback).NotEmpty().MaximumLength(4000);
    }
}
