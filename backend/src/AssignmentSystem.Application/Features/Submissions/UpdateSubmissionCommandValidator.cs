using FluentValidation;

namespace AssignmentSystem.Application.Features.Submissions;

public class UpdateSubmissionCommandValidator : AbstractValidator<UpdateSubmissionCommand>
{
    public UpdateSubmissionCommandValidator()
    {
        RuleFor(x => x.Content).NotEmpty().MaximumLength(8000);
        RuleFor(x => x.FileUrl).MaximumLength(1000);
    }
}
