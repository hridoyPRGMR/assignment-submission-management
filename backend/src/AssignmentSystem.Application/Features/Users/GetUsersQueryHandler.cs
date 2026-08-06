using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Application.Common;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Users;

public class GetUsersQueryHandler(IAppDbContext dbContext)
    : IRequestHandler<GetUsersQuery, Result<PagedResult<UserDto>>>
{
    public async Task<Result<PagedResult<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Users.AsNoTracking().OrderBy(u => u.FullName).AsQueryable();

        if (request.Role.HasValue)
        {
            query = query.Where(u => u.Role == request.Role.Value);
        }

        var paged = await query
            .Select(u => u)
            .ToPagedResultAsync(request.Page, request.PageSize, cancellationToken);

        var dtoPage = new PagedResult<UserDto>(
            paged.Items.Select(u => u.ToDto()).ToList(),
            paged.Page,
            paged.PageSize,
            paged.TotalCount);

        return Result.Success(dtoPage);
    }
}
