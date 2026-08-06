using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Users;

public record UpdateUserCommand(Guid Id, string FullName) : IRequest<Result<UserDto>>;
