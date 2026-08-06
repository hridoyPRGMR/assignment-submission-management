using AssignmentSystem.Domain.Common;

namespace AssignmentSystem.Application.Features.ClassSubjects;

public interface IClassSubjectService
{
    Task<Result<ClassSubjectDto>> AssignAsync(AssignSubjectToClassInput input, CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<ClassSubjectDto>>> GetForClassAsync(Guid? classId, Guid? subjectId, CancellationToken cancellationToken);

    Task<Result> RemoveAsync(Guid id, CancellationToken cancellationToken);
}
