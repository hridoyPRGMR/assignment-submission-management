using AssignmentSystem.Application.Abstractions;
using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Users;

public class CreateUserCommandHandler(IAppDbContext dbContext, IPasswordHasher passwordHasher)
    : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var emailInUse = await dbContext.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (emailInUse)
        {
            return Result.Failure<UserDto>("A user with this email already exists.");
        }

        var passwordHash = passwordHasher.Hash(request.Password);

        var userResult = User.Create(request.Email, passwordHash, request.FullName, request.Role, DateTime.UtcNow);
        if (userResult.IsFailure)
        {
            return Result.Failure<UserDto>(userResult.Error!);
        }

        dbContext.Users.Add(userResult.Value);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(userResult.Value.ToDto());
    }
}
