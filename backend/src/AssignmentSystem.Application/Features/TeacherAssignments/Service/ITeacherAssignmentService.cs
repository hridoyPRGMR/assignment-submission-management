using AssignmentSystem.Domain.Common;

namespace AssignmentSystem.Application.Features.TeacherAssignments;

public interface ITeacherAssignmentService
{
    Task<Result<TeacherAssignmentDto>> AssignAsync(AssignTeacherInput input, Guid actorUserId, CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<TeacherAssignmentDto>>> GetForTeacherAsync(Guid? teacherUserId, Guid? classId, Guid? subjectId, CancellationToken cancellationToken);

    Task<Result> RemoveAsync(Guid id, CancellationToken cancellationToken);
}
