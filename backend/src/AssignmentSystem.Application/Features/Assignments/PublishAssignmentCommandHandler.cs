using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Assignments;

public class PublishAssignmentCommandHandler(IAppDbContext dbContext)
    : IRequestHandler<PublishAssignmentCommand, Result<AssignmentDto>>
{
    public async Task<Result<AssignmentDto>> Handle(PublishAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await dbContext.Assignments
            .Include(a => a.Class)
            .Include(a => a.Subject)
            .Include(a => a.TeacherUser)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (assignment is null)
        {
            return Result.Failure<AssignmentDto>("Assignment not found.");
        }

        if (assignment.TeacherUserId != request.RequestingUserId)
        {
            return Result.Failure<AssignmentDto>("You do not own this assignment.");
        }

        var publishResult = assignment.Publish();
        if (publishResult.IsFailure)
        {
            return Result.Failure<AssignmentDto>(publishResult.Error!);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(assignment.ToDto());
    }
}
