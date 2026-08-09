using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.Features.Users;

public record UpdateUserInput(Guid Id, string FullName, UserRole? Role);
