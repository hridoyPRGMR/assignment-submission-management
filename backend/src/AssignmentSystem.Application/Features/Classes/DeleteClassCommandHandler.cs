using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Classes;

public class DeleteClassCommandHandler(IAppDbContext dbContext) : IRequestHandler<DeleteClassCommand, Result>
{
    public async Task<Result> Handle(DeleteClassCommand request, CancellationToken cancellationToken)
    {
        var classEntity = await dbContext.Classes.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (classEntity is null)
        {
            return Result.Failure("Class not found.");
        }

        var hasAssignments = await dbContext.Assignments.AnyAsync(a => a.ClassId == request.Id, cancellationToken);
        var hasEnrollments = await dbContext.StudentEnrollments.AnyAsync(e => e.ClassId == request.Id, cancellationToken);

        if (hasAssignments || hasEnrollments)
        {
            return Result.Failure("Cannot delete a class that has assignments or enrolled students.");
        }

        dbContext.Classes.Remove(classEntity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
