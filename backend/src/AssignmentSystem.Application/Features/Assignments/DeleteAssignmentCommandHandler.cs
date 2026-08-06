using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Assignments;

public class DeleteAssignmentCommandHandler(IAppDbContext dbContext) : IRequestHandler<DeleteAssignmentCommand, Result>
{
    public async Task<Result> Handle(DeleteAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await dbContext.Assignments.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (assignment is null)
        {
            return Result.Failure("Assignment not found.");
        }

        if (assignment.TeacherUserId != request.RequestingUserId)
        {
            return Result.Failure("You do not own this assignment.");
        }

        assignment.MarkDeleted(DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
