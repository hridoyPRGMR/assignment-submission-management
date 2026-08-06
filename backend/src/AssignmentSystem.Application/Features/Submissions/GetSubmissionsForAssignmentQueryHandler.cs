using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Submissions;

public class GetSubmissionsForAssignmentQueryHandler(IAppDbContext dbContext)
    : IRequestHandler<GetSubmissionsForAssignmentQuery, Result<IReadOnlyList<SubmissionDto>>>
{
    public async Task<Result<IReadOnlyList<SubmissionDto>>> Handle(
        GetSubmissionsForAssignmentQuery request, CancellationToken cancellationToken)
    {
        var assignment = await dbContext.Assignments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.AssignmentId, cancellationToken);

        if (assignment is null)
        {
            return Result.Failure<IReadOnlyList<SubmissionDto>>("Assignment not found.");
        }

        if (!request.IsAdmin && assignment.TeacherUserId != request.RequestingUserId)
        {
            return Result.Failure<IReadOnlyList<SubmissionDto>>("You do not own this assignment.");
        }

        var submissions = await dbContext.Submissions
            .AsNoTracking()
            .Include(s => s.Assignment)
            .Include(s => s.StudentUser)
            .Where(s => s.AssignmentId == request.AssignmentId)
            .OrderBy(s => s.StudentUser.FullName)
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<SubmissionDto>>(submissions.Select(s => s.ToDto()).ToList());
    }
}
