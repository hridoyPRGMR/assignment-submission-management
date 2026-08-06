using AssignmentSystem.Domain.Common;

namespace AssignmentSystem.Application.Features.Auth;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> LoginAsync(LoginInput input, CancellationToken cancellationToken);

    Task<Result> LogoutAsync(LogoutInput input, CancellationToken cancellationToken);

    Task<Result<AuthResponseDto>> RefreshTokenAsync(RefreshTokenInput input, CancellationToken cancellationToken);
}
