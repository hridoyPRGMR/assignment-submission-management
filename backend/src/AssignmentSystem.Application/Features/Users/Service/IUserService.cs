using AssignmentSystem.Application.Common;
using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.Features.Users;

public interface IUserService
{
    Task<Result<UserDto>> CreateAsync(CreateUserInput input, Guid actorUserId, CancellationToken cancellationToken);

    Task<Result<UserDto>> UpdateAsync(UpdateUserInput input, Guid actorUserId, CancellationToken cancellationToken);

    Task<Result> SetActiveStatusAsync(Guid id, bool isActive, Guid actorUserId, CancellationToken cancellationToken);

    Task<Result<UserDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<PagedResult<UserDto>>> GetListAsync(int page, int pageSize, UserRole? role, CancellationToken cancellationToken);
}
