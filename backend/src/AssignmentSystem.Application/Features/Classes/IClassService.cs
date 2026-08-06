using AssignmentSystem.Application.Common;
using AssignmentSystem.Domain.Common;

namespace AssignmentSystem.Application.Features.Classes;

public interface IClassService
{
    Task<Result<ClassDto>> CreateAsync(CreateClassInput input, CancellationToken cancellationToken);

    Task<Result<ClassDto>> UpdateAsync(UpdateClassInput input, CancellationToken cancellationToken);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<ClassDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<PagedResult<ClassDto>>> GetListAsync(int page, int pageSize, CancellationToken cancellationToken);
}
