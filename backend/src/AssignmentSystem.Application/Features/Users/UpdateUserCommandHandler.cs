using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Users;

public class UpdateUserCommandHandler(IAppDbContext dbContext) : IRequestHandler<UpdateUserCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserDto>("User not found.");
        }

        user.UpdateProfile(request.FullName);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(user.ToDto());
    }
}
