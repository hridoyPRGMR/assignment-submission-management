using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.TeacherAssignments;

public class RemoveTeacherAssignmentCommandHandler(IAppDbContext dbContext) : IRequestHandler<RemoveTeacherAssignmentCommand, Result>
{
    public async Task<Result> Handle(RemoveTeacherAssignmentCommand request, CancellationToken cancellationToken)
    {
        var teacherAssignment = await dbContext.TeacherAssignments.FirstOrDefaultAsync(ta => ta.Id == request.Id, cancellationToken);

        if (teacherAssignment is null)
        {
            return Result.Failure("Teacher assignment not found.");
        }

        dbContext.TeacherAssignments.Remove(teacherAssignment);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
