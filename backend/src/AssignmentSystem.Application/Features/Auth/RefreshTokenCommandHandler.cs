using AssignmentSystem.Application.Abstractions;
using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Application.Common;
using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AssignmentSystem.Application.Features.Auth;

public class RefreshTokenCommandHandler(
    IAppDbContext dbContext,
    IJwtTokenGenerator jwtTokenGenerator,
    IConfiguration configuration)
    : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
{
    public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;
        var tokenHash = TokenHasher.Hash(request.RefreshToken);

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
