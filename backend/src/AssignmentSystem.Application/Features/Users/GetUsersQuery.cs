using AssignmentSystem.Application.Common;
using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Enums;
using MediatR;

namespace AssignmentSystem.Application.Features.Users;

public record GetUsersQuery(int Page, int PageSize, UserRole? Role) : IRequest<Result<PagedResult<UserDto>>>;
