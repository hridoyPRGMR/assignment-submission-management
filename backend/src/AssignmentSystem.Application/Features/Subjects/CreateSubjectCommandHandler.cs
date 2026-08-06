using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Subjects;

public class CreateSubjectCommandHandler(IAppDbContext dbContext) : IRequestHandler<CreateSubjectCommand, Result<SubjectDto>>
{
    public async Task<Result<SubjectDto>> Handle(CreateSubjectCommand request, CancellationToken cancellationToken)
    {
        var subjectResult = Domain.Entities.Subject.Create(request.Name, request.Description, DateTime.UtcNow);
        if (subjectResult.IsFailure)
        {
            return Result.Failure<SubjectDto>(subjectResult.Error!);
        }

        dbContext.Subjects.Add(subjectResult.Value);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(subjectResult.Value.ToDto());
    }
}
