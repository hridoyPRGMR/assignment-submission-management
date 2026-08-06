using AssignmentSystem.Application.Abstractions;
using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Application.Common;
using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AssignmentSystem.Application.Features.Auth;

public class AuthService(
    IAppDbContext dbContext,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    IConfiguration configuration,
    IValidator<LoginInput> loginValidator,
    IValidator<LogoutInput> logoutValidator,
    IValidator<RefreshTokenInput> refreshTokenValidator)
    : IAuthService
{
    public async Task<Result<AuthResponseDto>> LoginAsync(LoginInput input, CancellationToken cancellationToken)
    {
        var validation = await loginValidator.ValidateAsync(input, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure<AuthResponseDto>(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage)));
        }

        var normalizedEmail = input.Email.Trim().ToLowerInvariant();

        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user is null || !user.IsActive || !passwordHasher.Verify(input.Password, user.PasswordHash))
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

    public async Task<Result> LogoutAsync(LogoutInput input, CancellationToken cancellationToken)
    {
        var validation = await logoutValidator.ValidateAsync(input, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage)));
        }

        var tokenHash = TokenHasher.Hash(input.RefreshToken);

        var existingToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (existingToken is not null && existingToken.IsActive(DateTime.UtcNow))
        {
            existingToken.Revoke(DateTime.UtcNow);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result<AuthResponseDto>> RefreshTokenAsync(RefreshTokenInput input, CancellationToken cancellationToken)
    {
        var validation = await refreshTokenValidator.ValidateAsync(input, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure<AuthResponseDto>(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage)));
        }

        var nowUtc = DateTime.UtcNow;
        var tokenHash = TokenHasher.Hash(input.RefreshToken);

        var existingToken = await dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (existingToken is null || !existingToken.IsActive(nowUtc) || !existingToken.User.IsActive)
        {
            return Result.Failure<AuthResponseDto>("Invalid or expired refresh token.");
        }

        existingToken.Revoke(nowUtc);

        var accessToken = jwtTokenGenerator.GenerateAccessToken(existingToken.User);

        var refreshTokenDays = configuration.GetValue("Jwt:RefreshTokenDays", 7);
        var plainRefreshToken = TokenHasher.GenerateToken();

        var newRefreshToken = RefreshToken.Create(
            existingToken.User.Id,
            TokenHasher.Hash(plainRefreshToken),
            nowUtc.AddDays(refreshTokenDays),
            nowUtc);

        dbContext.RefreshTokens.Add(newRefreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new AuthResponseDto(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            plainRefreshToken,
            existingToken.User.Id,
            existingToken.User.Email,
            existingToken.User.FullName,
            existingToken.User.Role.ToString()));
    }
}
