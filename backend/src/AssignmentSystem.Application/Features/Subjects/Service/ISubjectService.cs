using AssignmentSystem.Application.Common;
using AssignmentSystem.Domain.Common;

namespace AssignmentSystem.Application.Features.Subjects;

public interface ISubjectService
{
    Task<Result<SubjectDto>> CreateAsync(CreateSubjectInput input, Guid actorUserId, CancellationToken cancellationToken);

    Task<Result<SubjectDto>> UpdateAsync(UpdateSubjectInput input, Guid actorUserId, CancellationToken cancellationToken);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<SubjectDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<PagedResult<SubjectDto>>> GetListAsync(int page, int pageSize,string? search, string? sort, CancellationToken cancellationToken);
}
