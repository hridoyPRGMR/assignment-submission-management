using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Submissions;

public class CreateSubmissionCommandHandler(IAppDbContext dbContext)
    : IRequestHandler<CreateSubmissionCommand, Result<SubmissionDto>>
{
    public async Task<Result<SubmissionDto>> Handle(CreateSubmissionCommand request, CancellationToken cancellationToken)
    {
        var assignment = await dbContext.Assignments.FirstOrDefaultAsync(a => a.Id == request.AssignmentId, cancellationToken);

        if (assignment is null || assignment.Status != AssignmentStatus.Published)
        {
            return Result.Failure<SubmissionDto>("Assignment not found.");
        }

        var isEnrolled = await dbContext.StudentEnrollments.AnyAsync(
            e => e.StudentUserId == request.StudentUserId && e.ClassId == assignment.ClassId, cancellationToken);

        if (!isEnrolled)
        {
            return Result.Failure<SubmissionDto>("You are not enrolled in the class this assignment belongs to.");
        }

        var alreadySubmitted = await dbContext.Submissions.AnyAsync(
            s => s.AssignmentId == request.AssignmentId && s.StudentUserId == request.StudentUserId, cancellationToken);

        if (alreadySubmitted)
        {
            return Result.Failure<SubmissionDto>("You have already submitted this assignment.");
        }

        var submissionResult = Domain.Entities.Submission.Create(
            request.AssignmentId, request.StudentUserId, request.Content, request.FileUrl, assignment.DeadlineUtc, DateTime.UtcNow);

        if (submissionResult.IsFailure)
        {
            return Result.Failure<SubmissionDto>(submissionResult.Error!);
        }

        dbContext.Submissions.Add(submissionResult.Value);
        await dbContext.SaveChangesAsync(cancellationToken);

        var created = await dbContext.Submissions
            .Include(s => s.Assignment)
            .Include(s => s.StudentUser)
            .FirstAsync(s => s.Id == submissionResult.Value.Id, cancellationToken);

        return Result.Success(created.ToDto());
    }
}
