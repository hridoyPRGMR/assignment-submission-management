using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Application.Common;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Assignments;

public class GetAssignmentsQueryHandler(IAppDbContext dbContext)
    : IRequestHandler<GetAssignmentsQuery, Result<PagedResult<AssignmentDto>>>
{
    public async Task<Result<PagedResult<AssignmentDto>>> Handle(GetAssignmentsQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Assignments
            .AsNoTracking()
            .Include(a => a.Class)
            .Include(a => a.Subject)
            .Include(a => a.TeacherUser)
            .AsQueryable();

        if (request.TeacherUserId.HasValue)
        {
            query = query.Where(a => a.TeacherUserId == request.TeacherUserId.Value);
        }

        if (request.ClassId.HasValue)
        {
            query = query.Where(a => a.ClassId == request.ClassId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(a => a.Status == request.Status.Value);
        }

        query = query.OrderByDescending(a => a.CreatedAtUtc);

        var paged = await query.ToPagedResultAsync(request.Page, request.PageSize, cancellationToken);

        var dtoPage = new PagedResult<AssignmentDto>(
            paged.Items.Select(a => a.ToDto()).ToList(),
            paged.Page,
            paged.PageSize,
            paged.TotalCount);

        return Result.Success(dtoPage);
    }
}
