using AssignmentSystem.Domain.Common;

namespace AssignmentSystem.Application.Features.StudentEnrollments;

public interface IStudentEnrollmentService
{
    Task<Result<StudentEnrollmentDto>> EnrollAsync(EnrollStudentInput input, CancellationToken cancellationToken);

    Task<Result> UnenrollAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<StudentEnrollmentDto>>> GetForStudentAsync(Guid? studentUserId, Guid? classId, CancellationToken cancellationToken);

    Task<Result<bool>> IsEnrolledAsync(Guid studentUserId, Guid classId, CancellationToken cancellationToken);
}
