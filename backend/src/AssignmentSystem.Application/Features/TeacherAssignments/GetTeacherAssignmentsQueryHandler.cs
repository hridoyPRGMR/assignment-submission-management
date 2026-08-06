using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.TeacherAssignments;

public class GetTeacherAssignmentsQueryHandler(IAppDbContext dbContext)
    : IRequestHandler<GetTeacherAssignmentsQuery, Result<IReadOnlyList<TeacherAssignmentDto>>>
{
    public async Task<Result<IReadOnlyList<TeacherAssignmentDto>>> Handle(GetTeacherAssignmentsQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.TeacherAssignments
            .AsNoTracking()
            .Include(ta => ta.TeacherUser)
            .Include(ta => ta.Class)
            .Include(ta => ta.Subject)
            .AsQueryable();

        if (request.TeacherUserId.HasValue)
        {
            query = query.Where(ta => ta.TeacherUserId == request.TeacherUserId.Value);
        }

        if (request.ClassId.HasValue)
        {
            query = query.Where(ta => ta.ClassId == request.ClassId.Value);
        }

        if (request.SubjectId.HasValue)
        {
            query = query.Where(ta => ta.SubjectId == request.SubjectId.Value);
        }

        var items = await query
            .Select(ta => new TeacherAssignmentDto(
                ta.Id, ta.TeacherUserId, ta.TeacherUser.FullName, ta.ClassId, ta.Class.Name, ta.SubjectId, ta.Subject.Name))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<TeacherAssignmentDto>>(items);
    }
}
