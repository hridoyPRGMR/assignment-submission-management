using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Application.Common;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Auth;

public class LogoutCommandHandler(IAppDbContext dbContext) : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = TokenHasher.Hash(request.RefreshToken);

        var existingToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (existingToken is not null && existingToken.IsActive(DateTime.UtcNow))
        {
            existingToken.Revoke(DateTime.UtcNow);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
