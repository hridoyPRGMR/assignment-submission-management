using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.StudentEnrollments;

public class EnrollStudentCommandHandler(IAppDbContext dbContext)
    : IRequestHandler<EnrollStudentCommand, Result<StudentEnrollmentDto>>
{
    public async Task<Result<StudentEnrollmentDto>> Handle(EnrollStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.StudentUserId, cancellationToken);
        if (student is null || student.Role != UserRole.Student || !student.IsActive)
        {
            return Result.Failure<StudentEnrollmentDto>("Student not found or is not an active student account.");
        }

        var classEntity = await dbContext.Classes.FirstOrDefaultAsync(c => c.Id == request.ClassId, cancellationToken);
        if (classEntity is null)
        {
            return Result.Failure<StudentEnrollmentDto>("Class not found.");
        }

        var alreadyEnrolled = await dbContext.StudentEnrollments.AnyAsync(
            e => e.StudentUserId == request.StudentUserId && e.ClassId == request.ClassId, cancellationToken);

        if (alreadyEnrolled)
        {
            return Result.Failure<StudentEnrollmentDto>("This student is already enrolled in this class.");
        }

        var enrollment = StudentEnrollment.Create(request.StudentUserId, request.ClassId);
        dbContext.StudentEnrollments.Add(enrollment);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new StudentEnrollmentDto(enrollment.Id, student.Id, student.FullName, classEntity.Id, classEntity.Name));
    }
}
