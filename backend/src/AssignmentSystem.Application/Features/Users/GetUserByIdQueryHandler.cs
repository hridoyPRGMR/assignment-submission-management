using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Users;

public class GetUserByIdQueryHandler(IAppDbContext dbContext) : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        return user is null
            ? Result.Failure<UserDto>("User not found.")
            : Result.Success(user.ToDto());
    }
}
