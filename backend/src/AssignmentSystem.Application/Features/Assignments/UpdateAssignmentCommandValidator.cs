using FluentValidation;

namespace AssignmentSystem.Application.Features.Assignments;

public class UpdateAssignmentCommandValidator : AbstractValidator<UpdateAssignmentCommand>
{
    public UpdateAssignmentCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.MaxMarks).GreaterThan(0);
        RuleFor(x => x.DeadlineUtc).GreaterThan(DateTime.UtcNow);
    }
}
