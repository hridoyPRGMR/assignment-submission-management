using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Subjects;

public class DeleteSubjectCommandHandler(IAppDbContext dbContext) : IRequestHandler<DeleteSubjectCommand, Result>
{
    public async Task<Result> Handle(DeleteSubjectCommand request, CancellationToken cancellationToken)
    {
        var subject = await dbContext.Subjects.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (subject is null)
        {
            return Result.Failure("Subject not found.");
        }

        var hasAssignments = await dbContext.Assignments.AnyAsync(a => a.SubjectId == request.Id, cancellationToken);

        if (hasAssignments)
        {
            return Result.Failure("Cannot delete a subject that has existing assignments.");
        }

        dbContext.Subjects.Remove(subject);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
