using FluentValidation;

namespace AssignmentSystem.Application.Features.Submissions;

public class UpdateSubmissionInputValidator : AbstractValidator<UpdateSubmissionInput>
{
    public UpdateSubmissionInputValidator()
    {
        RuleFor(x => x.Content).NotEmpty().MaximumLength(8000);
        RuleFor(x => x.FileName).MaximumLength(260);
    }
}
