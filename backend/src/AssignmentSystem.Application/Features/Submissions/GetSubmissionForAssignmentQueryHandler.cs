using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Submissions;

public class GetSubmissionForAssignmentQueryHandler(IAppDbContext dbContext)
    : IRequestHandler<GetSubmissionForAssignmentQuery, Result<SubmissionDto?>>
{
    public async Task<Result<SubmissionDto?>> Handle(GetSubmissionForAssignmentQuery request, CancellationToken cancellationToken)
    {
        var submission = await dbContext.Submissions
            .AsNoTracking()
            .Include(s => s.Assignment)
            .Include(s => s.StudentUser)
            .FirstOrDefaultAsync(
                s => s.AssignmentId == request.AssignmentId && s.StudentUserId == request.StudentUserId, cancellationToken);

        return Result.Success(submission?.ToDto());
    }
}
