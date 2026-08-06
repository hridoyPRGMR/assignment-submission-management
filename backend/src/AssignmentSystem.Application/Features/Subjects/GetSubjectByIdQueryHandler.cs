using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Subjects;

public class GetSubjectByIdQueryHandler(IAppDbContext dbContext) : IRequestHandler<GetSubjectByIdQuery, Result<SubjectDto>>
{
    public async Task<Result<SubjectDto>> Handle(GetSubjectByIdQuery request, CancellationToken cancellationToken)
    {
        var subject = await dbContext.Subjects.AsNoTracking().FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        return subject is null
            ? Result.Failure<SubjectDto>("Subject not found.")
            : Result.Success(subject.ToDto());
    }
}
