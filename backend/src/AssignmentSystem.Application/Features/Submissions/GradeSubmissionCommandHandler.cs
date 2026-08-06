using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Submissions;

public class GradeSubmissionCommandHandler(IAppDbContext dbContext) : IRequestHandler<GradeSubmissionCommand, Result<SubmissionDto>>
{
    public async Task<Result<SubmissionDto>> Handle(GradeSubmissionCommand request, CancellationToken cancellationToken)
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

        var result = submission.Grade(
            request.Marks, request.Feedback, submission.Assignment.MaxMarks, request.RequestingTeacherUserId, DateTime.UtcNow);

        if (result.IsFailure)
        {
            return Result.Failure<SubmissionDto>(result.Error!);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(submission.ToDto());
    }
}
