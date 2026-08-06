using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.StudentEnrollments;

public class GetStudentEnrollmentsQueryHandler(IAppDbContext dbContext)
    : IRequestHandler<GetStudentEnrollmentsQuery, Result<IReadOnlyList<StudentEnrollmentDto>>>
{
    public async Task<Result<IReadOnlyList<StudentEnrollmentDto>>> Handle(GetStudentEnrollmentsQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.StudentEnrollments
            .AsNoTracking()
            .Include(e => e.StudentUser)
            .Include(e => e.Class)
            .AsQueryable();

        if (request.StudentUserId.HasValue)
        {
            query = query.Where(e => e.StudentUserId == request.StudentUserId.Value);
        }

        if (request.ClassId.HasValue)
        {
            query = query.Where(e => e.ClassId == request.ClassId.Value);
        }

        var items = await query
            .Select(e => new StudentEnrollmentDto(e.Id, e.StudentUserId, e.StudentUser.FullName, e.ClassId, e.Class.Name))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<StudentEnrollmentDto>>(items);
    }
}
