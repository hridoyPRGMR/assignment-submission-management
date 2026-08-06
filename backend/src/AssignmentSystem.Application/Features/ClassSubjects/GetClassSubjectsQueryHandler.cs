using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.ClassSubjects;

public class GetClassSubjectsQueryHandler(IAppDbContext dbContext)
    : IRequestHandler<GetClassSubjectsQuery, Result<IReadOnlyList<ClassSubjectDto>>>
{
    public async Task<Result<IReadOnlyList<ClassSubjectDto>>> Handle(GetClassSubjectsQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.ClassSubjects
            .AsNoTracking()
            .Include(cs => cs.Class)
            .Include(cs => cs.Subject)
            .AsQueryable();

        if (request.ClassId.HasValue)
        {
            query = query.Where(cs => cs.ClassId == request.ClassId.Value);
        }

        if (request.SubjectId.HasValue)
        {
            query = query.Where(cs => cs.SubjectId == request.SubjectId.Value);
        }

        var items = await query
            .Select(cs => new ClassSubjectDto(cs.Id, cs.ClassId, cs.Class.Name, cs.SubjectId, cs.Subject.Name))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<ClassSubjectDto>>(items);
    }
}
