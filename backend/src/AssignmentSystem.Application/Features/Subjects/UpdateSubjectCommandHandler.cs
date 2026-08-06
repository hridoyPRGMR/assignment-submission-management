using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Subjects;

public class UpdateSubjectCommandHandler(IAppDbContext dbContext) : IRequestHandler<UpdateSubjectCommand, Result<SubjectDto>>
{
    public async Task<Result<SubjectDto>> Handle(UpdateSubjectCommand request, CancellationToken cancellationToken)
    {
        var subject = await dbContext.Subjects.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (subject is null)
        {
            return Result.Failure<SubjectDto>("Subject not found.");
        }

        subject.Update(request.Name, request.Description);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(subject.ToDto());
    }
}
