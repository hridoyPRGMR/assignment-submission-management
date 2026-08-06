using FluentValidation;

namespace AssignmentSystem.Application.Features.Submissions;

public class CreateSubmissionInputValidator : AbstractValidator<CreateSubmissionInput>
{
    public CreateSubmissionInputValidator()
    {
        RuleFor(x => x.AssignmentId).NotEmpty();
        RuleFor(x => x.Content).NotEmpty().MaximumLength(8000);
        RuleFor(x => x.FileName).MaximumLength(260);
    }
}
