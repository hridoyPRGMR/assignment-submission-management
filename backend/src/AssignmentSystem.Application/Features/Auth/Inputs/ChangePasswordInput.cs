namespace AssignmentSystem.Application.Features.Auth;

public record ChangePasswordInput(Guid UserId, string CurrentPassword, string NewPassword);
