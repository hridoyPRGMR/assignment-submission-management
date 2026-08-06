using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.StudentEnrollments;

public class StudentEnrollmentService(IAppDbContext dbContext, IValidator<EnrollStudentInput> enrollValidator)
    : IStudentEnrollmentService
{
    public async Task<Result<StudentEnrollmentDto>> EnrollAsync(EnrollStudentInput input, CancellationToken cancellationToken)
    {
        var validation = await enrollValidator.ValidateAsync(input, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure<StudentEnrollmentDto>(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage)));
        }

        var student = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == input.StudentUserId, cancellationToken);
        if (student is null || student.Role != UserRole.Student || !student.IsActive)
        {
            return Result.Failure<StudentEnrollmentDto>("Student not found or is not an active student account.");
        }

        var classEntity = await dbContext.Classes.FirstOrDefaultAsync(c => c.Id == input.ClassId, cancellationToken);
        if (classEntity is null)
        {
            return Result.Failure<StudentEnrollmentDto>("Class not found.");
        }

        var alreadyEnrolled = await dbContext.StudentEnrollments.AnyAsync(
            e => e.StudentUserId == input.StudentUserId && e.ClassId == input.ClassId, cancellationToken);

        if (alreadyEnrolled)
        {
            return Result.Failure<StudentEnrollmentDto>("This student is already enrolled in this class.");
        }

        var enrollment = StudentEnrollment.Create(input.StudentUserId, input.ClassId);
        dbContext.StudentEnrollments.Add(enrollment);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new StudentEnrollmentDto(enrollment.Id, student.Id, student.FullName, classEntity.Id, classEntity.Name));
    }

    public async Task<Result> UnenrollAsync(Guid id, CancellationToken cancellationToken)
    {
        var enrollment = await dbContext.StudentEnrollments.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

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

    public async Task<Result<IReadOnlyList<StudentEnrollmentDto>>> GetForStudentAsync(Guid? studentUserId, Guid? classId, CancellationToken cancellationToken)
    {
        var query = dbContext.StudentEnrollments
            .AsNoTracking()
            .Include(e => e.StudentUser)
            .Include(e => e.Class)
            .AsQueryable();

        if (studentUserId.HasValue)
        {
            query = query.Where(e => e.StudentUserId == studentUserId.Value);
        }

        if (classId.HasValue)
        {
            query = query.Where(e => e.ClassId == classId.Value);
        }

        var items = await query
            .Select(e => new StudentEnrollmentDto(e.Id, e.StudentUserId, e.StudentUser.FullName, e.ClassId, e.Class.Name))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<StudentEnrollmentDto>>(items);
    }

    public async Task<Result<bool>> IsEnrolledAsync(Guid studentUserId, Guid classId, CancellationToken cancellationToken)
    {
        var isEnrolled = await dbContext.StudentEnrollments.AnyAsync(
            e => e.StudentUserId == studentUserId && e.ClassId == classId, cancellationToken);

        return Result.Success(isEnrolled);
    }
}
