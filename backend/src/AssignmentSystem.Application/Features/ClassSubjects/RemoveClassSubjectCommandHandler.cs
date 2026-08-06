using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.ClassSubjects;

public class RemoveClassSubjectCommandHandler(IAppDbContext dbContext) : IRequestHandler<RemoveClassSubjectCommand, Result>
{
    public async Task<Result> Handle(RemoveClassSubjectCommand request, CancellationToken cancellationToken)
    {
        var classSubject = await dbContext.ClassSubjects.FirstOrDefaultAsync(cs => cs.Id == request.Id, cancellationToken);

        if (classSubject is null)
        {
            return Result.Failure("Class-subject association not found.");
        }

        var hasTeacherAssignments = await dbContext.TeacherAssignments.AnyAsync(
            ta => ta.ClassId == classSubject.ClassId && ta.SubjectId == classSubject.SubjectId, cancellationToken);

        if (hasTeacherAssignments)
        {
            return Result.Failure("Cannot remove this association while teachers are assigned to it.");
        }

        dbContext.ClassSubjects.Remove(classSubject);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
