namespace AssignmentSystem.Application.Features.Auth;

public record AuthResponseDto(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    Guid UserId,
    string Email,
    string FullName,
    string Role);
