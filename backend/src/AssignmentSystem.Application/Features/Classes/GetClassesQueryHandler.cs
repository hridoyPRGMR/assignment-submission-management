using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Application.Common;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Classes;

public class GetClassesQueryHandler(IAppDbContext dbContext)
    : IRequestHandler<GetClassesQuery, Result<PagedResult<ClassDto>>>
{
    public async Task<Result<PagedResult<ClassDto>>> Handle(GetClassesQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Classes.AsNoTracking().OrderBy(c => c.Name);

        var paged = await query.ToPagedResultAsync(request.Page, request.PageSize, cancellationToken);

        var dtoPage = new PagedResult<ClassDto>(
            paged.Items.Select(c => c.ToDto()).ToList(),
            paged.Page,
            paged.PageSize,
            paged.TotalCount);

        return Result.Success(dtoPage);
    }
}
