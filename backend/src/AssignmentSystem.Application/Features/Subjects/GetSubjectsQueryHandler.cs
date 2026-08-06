using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Application.Common;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Subjects;

public class GetSubjectsQueryHandler(IAppDbContext dbContext)
    : IRequestHandler<GetSubjectsQuery, Result<PagedResult<SubjectDto>>>
{
    public async Task<Result<PagedResult<SubjectDto>>> Handle(GetSubjectsQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Subjects.AsNoTracking().OrderBy(s => s.Name);

        var paged = await query.ToPagedResultAsync(request.Page, request.PageSize, cancellationToken);

        var dtoPage = new PagedResult<SubjectDto>(
            paged.Items.Select(s => s.ToDto()).ToList(),
            paged.Page,
            paged.PageSize,
            paged.TotalCount);

        return Result.Success(dtoPage);
    }
}
