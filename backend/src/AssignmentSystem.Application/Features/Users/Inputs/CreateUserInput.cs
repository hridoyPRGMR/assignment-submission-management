using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.Features.Users;

public record CreateUserInput(string Email, string Password, string FullName, UserRole Role);
