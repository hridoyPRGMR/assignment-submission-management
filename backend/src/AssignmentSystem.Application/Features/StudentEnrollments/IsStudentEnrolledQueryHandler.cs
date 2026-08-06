using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.StudentEnrollments;

public class IsStudentEnrolledQueryHandler(IAppDbContext dbContext) : IRequestHandler<IsStudentEnrolledQuery, Result<bool>>
{
    public async Task<Result<bool>> Handle(IsStudentEnrolledQuery request, CancellationToken cancellationToken)
    {
        var isEnrolled = await dbContext.StudentEnrollments.AnyAsync(
            e => e.StudentUserId == request.StudentUserId && e.ClassId == request.ClassId, cancellationToken);

        return Result.Success(isEnrolled);
    }
}
