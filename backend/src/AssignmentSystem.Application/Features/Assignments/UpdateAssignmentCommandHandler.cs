using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Assignments;

public class UpdateAssignmentCommandHandler(IAppDbContext dbContext)
    : IRequestHandler<UpdateAssignmentCommand, Result<AssignmentDto>>
{
    public async Task<Result<AssignmentDto>> Handle(UpdateAssignmentCommand request, CancellationToken cancellationToken)
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

        var hasSubmissions = await dbContext.Submissions.AnyAsync(s => s.AssignmentId == request.Id, cancellationToken);

        var updateResult = assignment.UpdateDetails(
            request.Title, request.Description, request.DeadlineUtc, request.MaxMarks, hasSubmissions, DateTime.UtcNow);

        if (updateResult.IsFailure)
        {
            return Result.Failure<AssignmentDto>(updateResult.Error!);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(assignment.ToDto());
    }
}
