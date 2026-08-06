using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Enums;
using MediatR;

namespace AssignmentSystem.Application.Features.Users;

public record CreateUserCommand(string Email, string Password, string FullName, UserRole Role)
    : IRequest<Result<UserDto>>;
