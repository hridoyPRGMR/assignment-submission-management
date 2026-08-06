namespace AssignmentSystem.Application.Features.Users;

public record UserDto(
    Guid Id,
    string Email,
    string FullName,
    string Role,
    bool IsActive,
    DateTime CreatedAtUtc);
