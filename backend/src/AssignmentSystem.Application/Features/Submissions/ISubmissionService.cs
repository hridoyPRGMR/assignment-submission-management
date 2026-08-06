using AssignmentSystem.Domain.Common;

namespace AssignmentSystem.Application.Features.Submissions;

public interface ISubmissionService
{
    Task<Result<SubmissionDto>> CreateAsync(CreateSubmissionInput input, CancellationToken cancellationToken);

    Task<Result<SubmissionDto>> UpdateAsync(UpdateSubmissionInput input, CancellationToken cancellationToken);

    Task<Result<SubmissionDto>> GradeAsync(GradeSubmissionInput input, CancellationToken cancellationToken);

    Task<Result<SubmissionDto>> ReturnForRevisionAsync(ReturnSubmissionForRevisionInput input, CancellationToken cancellationToken);

    Task<Result<SubmissionDto>> StartReviewAsync(Guid submissionId, Guid requestingTeacherUserId, CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<SubmissionDto>>> GetMySubmissionsAsync(Guid studentUserId, CancellationToken cancellationToken);

    Task<Result<SubmissionDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<SubmissionDto?>> GetForAssignmentAsync(Guid assignmentId, Guid studentUserId, CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<SubmissionDto>>> GetListForAssignmentAsync(
        Guid assignmentId, Guid requestingUserId, bool isAdmin, CancellationToken cancellationToken);
}
