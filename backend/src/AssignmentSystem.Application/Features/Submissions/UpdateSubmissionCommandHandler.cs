using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Submissions;

public class UpdateSubmissionCommandHandler(IAppDbContext dbContext)
    : IRequestHandler<UpdateSubmissionCommand, Result<SubmissionDto>>
{
    public async Task<Result<SubmissionDto>> Handle(UpdateSubmissionCommand request, CancellationToken cancellationToken)
    {
        var submission = await dbContext.Submissions
            .Include(s => s.Assignment)
            .Include(s => s.StudentUser)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (submission is null)
        {
            return Result.Failure<SubmissionDto>("Submission not found.");
        }

        if (submission.StudentUserId != request.RequestingStudentUserId)
        {
            return Result.Failure<SubmissionDto>("You do not own this submission.");
        }

        var updateResult = submission.Update(request.Content, request.FileUrl, submission.Assignment.DeadlineUtc, DateTime.UtcNow);

        if (updateResult.IsFailure)
        {
            return Result.Failure<SubmissionDto>(updateResult.Error!);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(submission.ToDto());
    }
}
