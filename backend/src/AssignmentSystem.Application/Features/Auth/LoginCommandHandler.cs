using AssignmentSystem.Application.Abstractions;
using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Application.Common;
using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AssignmentSystem.Application.Features.Auth;

public class LoginCommandHandler(
    IAppDbContext dbContext,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    IConfiguration configuration)
    : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user is null || !user.IsActive || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Result.Failure<AuthResponseDto>("Invalid email or password.");
        }

        var accessToken = jwtTokenGenerator.GenerateAccessToken(user);

        var refreshTokenDays = configuration.GetValue("Jwt:RefreshTokenDays", 7);
        var nowUtc = DateTime.UtcNow;
        var plainRefreshToken = TokenHasher.GenerateToken();

        var refreshToken = RefreshToken.Create(
            user.Id,
            TokenHasher.Hash(plainRefreshToken),
            nowUtc.AddDays(refreshTokenDays),
            nowUtc);

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new AuthResponseDto(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            plainRefreshToken,
            user.Id,
            user.Email,
            user.FullName,
            user.Role.ToString()));
    }
}
