using FluentValidation;

namespace AssignmentSystem.Application.Features.Submissions;

public class GradeSubmissionCommandValidator : AbstractValidator<GradeSubmissionCommand>
{
    public GradeSubmissionCommandValidator()
    {
        RuleFor(x => x.Marks).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Feedback).NotEmpty().MaximumLength(4000);
    }
}
