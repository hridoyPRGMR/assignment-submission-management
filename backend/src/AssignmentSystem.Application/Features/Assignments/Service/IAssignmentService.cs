using AssignmentSystem.Application.Common;
using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.Features.Assignments;

public interface IAssignmentService
{
    Task<Result<AssignmentDto>> CreateAsync(CreateAssignmentInput input, CancellationToken cancellationToken);

    Task<Result<AssignmentDto>> UpdateAsync(UpdateAssignmentInput input, CancellationToken cancellationToken);

    Task<Result> DeleteAsync(Guid id, Guid requestingUserId, CancellationToken cancellationToken);

    Task<Result<AssignmentDto>> PublishAsync(Guid id, Guid requestingUserId, CancellationToken cancellationToken);

    Task<Result<AssignmentDto>> UnpublishAsync(Guid id, Guid requestingUserId, CancellationToken cancellationToken);

    Task<Result<AssignmentDto>> MarkGradedAsync(Guid id, Guid requestingUserId, CancellationToken cancellationToken);

    Task<Result<AssignmentDto>> ArchiveAsync(Guid id, Guid requestingUserId, CancellationToken cancellationToken);

    Task<Result<AssignmentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<PagedResult<AssignmentDto>>> GetListAsync(
        Guid? teacherUserId, Guid? classId, AssignmentStatus? status, int page, int pageSize, CancellationToken cancellationToken);

    Task<Result<PagedResult<AssignmentDto>>> GetPublishedForStudentAsync(
        Guid studentUserId, int page, int pageSize, CancellationToken cancellationToken);
}
