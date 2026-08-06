using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.StudentEnrollments;

public class UnenrollStudentCommandHandler(IAppDbContext dbContext) : IRequestHandler<UnenrollStudentCommand, Result>
{
    public async Task<Result> Handle(UnenrollStudentCommand request, CancellationToken cancellationToken)
    {
        var enrollment = await dbContext.StudentEnrollments.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (enrollment is null)
        {
            return Result.Failure("Enrollment not found.");
        }

        var hasSubmissions = await dbContext.Submissions.AnyAsync(
            s => s.StudentUserId == enrollment.StudentUserId && s.Assignment.ClassId == enrollment.ClassId,
            cancellationToken);

        if (hasSubmissions)
        {
            return Result.Failure("Cannot unenroll a student who has submissions in this class.");
        }

        dbContext.StudentEnrollments.Remove(enrollment);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
