using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Users;

public record GetUserByIdQuery(Guid Id) : IRequest<Result<UserDto>>;
