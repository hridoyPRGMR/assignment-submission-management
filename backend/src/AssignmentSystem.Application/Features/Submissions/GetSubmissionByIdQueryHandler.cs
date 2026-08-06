using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Submissions;

public class GetSubmissionByIdQueryHandler(IAppDbContext dbContext) : IRequestHandler<GetSubmissionByIdQuery, Result<SubmissionDto>>
{
    public async Task<Result<SubmissionDto>> Handle(GetSubmissionByIdQuery request, CancellationToken cancellationToken)
    {
        var submission = await dbContext.Submissions
            .AsNoTracking()
            .Include(s => s.Assignment)
            .Include(s => s.StudentUser)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        return submission is null
            ? Result.Failure<SubmissionDto>("Submission not found.")
            : Result.Success(submission.ToDto());
    }
}
