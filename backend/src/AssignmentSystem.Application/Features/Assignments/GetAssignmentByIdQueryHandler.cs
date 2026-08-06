using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Assignments;

public class GetAssignmentByIdQueryHandler(IAppDbContext dbContext) : IRequestHandler<GetAssignmentByIdQuery, Result<AssignmentDto>>
{
    public async Task<Result<AssignmentDto>> Handle(GetAssignmentByIdQuery request, CancellationToken cancellationToken)
    {
        var assignment = await dbContext.Assignments
            .AsNoTracking()
            .Include(a => a.Class)
            .Include(a => a.Subject)
            .Include(a => a.TeacherUser)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        return assignment is null
            ? Result.Failure<AssignmentDto>("Assignment not found.")
            : Result.Success(assignment.ToDto());
    }
}
