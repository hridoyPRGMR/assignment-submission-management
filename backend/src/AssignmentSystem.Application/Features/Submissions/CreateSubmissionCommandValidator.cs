using FluentValidation;

namespace AssignmentSystem.Application.Features.Submissions;

public class CreateSubmissionCommandValidator : AbstractValidator<CreateSubmissionCommand>
{
    public CreateSubmissionCommandValidator()
    {
        RuleFor(x => x.AssignmentId).NotEmpty();
        RuleFor(x => x.Content).NotEmpty().MaximumLength(8000);
        RuleFor(x => x.FileUrl).MaximumLength(1000);
    }
}
