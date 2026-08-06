using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Application.Common;
using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Assignments;

public class GetPublishedAssignmentsForStudentQueryHandler(IAppDbContext dbContext)
    : IRequestHandler<GetPublishedAssignmentsForStudentQuery, Result<PagedResult<AssignmentDto>>>
{
    public async Task<Result<PagedResult<AssignmentDto>>> Handle(
        GetPublishedAssignmentsForStudentQuery request, CancellationToken cancellationToken)
    {
        var classIds = await dbContext.StudentEnrollments
            .Where(e => e.StudentUserId == request.StudentUserId)
            .Select(e => e.ClassId)
            .ToListAsync(cancellationToken);

        var query = dbContext.Assignments
            .AsNoTracking()
            .Include(a => a.Class)
            .Include(a => a.Subject)
            .Include(a => a.TeacherUser)
            .Where(a => classIds.Contains(a.ClassId) && a.Status == AssignmentStatus.Published)
            .OrderByDescending(a => a.DeadlineUtc);

        var paged = await query.ToPagedResultAsync(request.Page, request.PageSize, cancellationToken);

        var dtoPage = new PagedResult<AssignmentDto>(
            paged.Items.Select(a => a.ToDto()).ToList(),
            paged.Page,
            paged.PageSize,
            paged.TotalCount);

        return Result.Success(dtoPage);
    }
}
