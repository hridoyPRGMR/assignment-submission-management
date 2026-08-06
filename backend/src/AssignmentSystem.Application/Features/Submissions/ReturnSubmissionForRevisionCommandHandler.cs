using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Submissions;

public class ReturnSubmissionForRevisionCommandHandler(IAppDbContext dbContext)
    : IRequestHandler<ReturnSubmissionForRevisionCommand, Result<SubmissionDto>>
{
    public async Task<Result<SubmissionDto>> Handle(ReturnSubmissionForRevisionCommand request, CancellationToken cancellationToken)
    {
        var submission = await dbContext.Submissions
            .Include(s => s.Assignment)
            .Include(s => s.StudentUser)
            .FirstOrDefaultAsync(s => s.Id == request.SubmissionId, cancellationToken);

        if (submission is null)
        {
            return Result.Failure<SubmissionDto>("Submission not found.");
        }

        if (submission.Assignment.TeacherUserId != request.RequestingTeacherUserId)
        {
            return Result.Failure<SubmissionDto>("You do not own this assignment.");
        }

        var result = submission.ReturnForRevision(request.Feedback);
        if (result.IsFailure)
        {
            return Result.Failure<SubmissionDto>(result.Error!);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(submission.ToDto());
    }
}
