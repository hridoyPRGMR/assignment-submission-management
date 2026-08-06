using FluentValidation;

namespace AssignmentSystem.Application.Features.Submissions;

public class ReturnSubmissionForRevisionInputValidator : AbstractValidator<ReturnSubmissionForRevisionInput>
{
    public ReturnSubmissionForRevisionInputValidator()
    {
        RuleFor(x => x.Feedback).NotEmpty().MaximumLength(4000);
    }
}
