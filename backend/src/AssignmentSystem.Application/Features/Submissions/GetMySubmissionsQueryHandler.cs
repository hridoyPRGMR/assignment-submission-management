using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Submissions;

public class GetMySubmissionsQueryHandler(IAppDbContext dbContext)
    : IRequestHandler<GetMySubmissionsQuery, Result<IReadOnlyList<SubmissionDto>>>
{
    public async Task<Result<IReadOnlyList<SubmissionDto>>> Handle(GetMySubmissionsQuery request, CancellationToken cancellationToken)
    {
        var submissions = await dbContext.Submissions
            .AsNoTracking()
            .Include(s => s.Assignment)
            .Include(s => s.StudentUser)
            .Where(s => s.StudentUserId == request.StudentUserId)
            .OrderByDescending(s => s.SubmittedAtUtc)
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<SubmissionDto>>(submissions.Select(s => s.ToDto()).ToList());
    }
}
