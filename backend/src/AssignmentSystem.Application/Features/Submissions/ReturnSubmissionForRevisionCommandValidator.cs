using FluentValidation;

namespace AssignmentSystem.Application.Features.Submissions;

public class ReturnSubmissionForRevisionCommandValidator : AbstractValidator<ReturnSubmissionForRevisionCommand>
{
    public ReturnSubmissionForRevisionCommandValidator()
    {
        RuleFor(x => x.Feedback).NotEmpty().MaximumLength(4000);
    }
}
