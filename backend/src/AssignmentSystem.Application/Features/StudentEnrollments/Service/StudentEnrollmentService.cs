using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AssignmentSystem.Application.Features.StudentEnrollments;

public class StudentEnrollmentService(
    IClassRepository classRepository,
    IStudentEnrollmentRepository studentEnrollmentRepository,
    ISubmissionRepository submissionRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IValidator<EnrollStudentInput> enrollValidator,
    ILogger<StudentEnrollmentService> logger)
    : IStudentEnrollmentService
{
    public async Task<Result<StudentEnrollmentDto>> EnrollAsync(EnrollStudentInput input, Guid actorUserId, CancellationToken cancellationToken)
    {
        var validation = await enrollValidator.ValidateAsync(input, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure<StudentEnrollmentDto>(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage)));
        }

        var student = await userRepository.Query().FirstOrDefaultAsync(u => u.Id == input.StudentUserId, cancellationToken);
        if (student is null || student.Role != UserRole.Student || !student.IsActive)
        {
            return Result.NotFound<StudentEnrollmentDto>("Student not found or is not an active student account.");
        }

        var classEntity = await classRepository.Query().FirstOrDefaultAsync(c => c.Id == input.ClassId, cancellationToken);
        if (classEntity is null)
        {
            return Result.NotFound<StudentEnrollmentDto>("Class not found.");
        }

        var alreadyEnrolled = await studentEnrollmentRepository.Query().AnyAsync(
            e => e.StudentUserId == input.StudentUserId && e.ClassId == input.ClassId, cancellationToken);

        if (alreadyEnrolled)
        {
            return Result.Failure<StudentEnrollmentDto>("This student is already enrolled in this class.");
        }

        var enrollment = StudentEnrollment.Create(input.StudentUserId, input.ClassId, DateTime.UtcNow, actorUserId);
        studentEnrollmentRepository.Add(enrollment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Admin {ActorUserId} enrolled student {StudentUserId} in class {ClassId} (enrollment {EnrollmentId})",
            actorUserId, input.StudentUserId, input.ClassId, enrollment.Id);

        return Result.Success(new StudentEnrollmentDto(enrollment.Id, student.Id, student.FullName, classEntity.Id, classEntity.Name));
    }

    public async Task<Result> UnenrollAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
    {
        var enrollment = await studentEnrollmentRepository.Query().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (enrollment is null)
        {
            return Result.NotFound("Enrollment not found.");
        }

        var hasSubmissions = await submissionRepository.Query().AnyAsync(
            s => s.StudentUserId == enrollment.StudentUserId && s.Assignment.ClassId == enrollment.ClassId,
            cancellationToken);

        if (hasSubmissions)
        {
            return Result.Failure("Cannot unenroll a student who has submissions in this class.");
        }

        studentEnrollmentRepository.Remove(enrollment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Admin {ActorUserId} unenrolled enrollment {EnrollmentId}", actorUserId, id);

        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<StudentEnrollmentDto>>> GetForStudentAsync(Guid? studentUserId, Guid? classId, CancellationToken cancellationToken)
    {
        var query = studentEnrollmentRepository.Query()
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
        var isEnrolled = await studentEnrollmentRepository.Query().AnyAsync(
            e => e.StudentUserId == studentUserId && e.ClassId == classId, cancellationToken);

        return Result.Success(isEnrolled);
    }
}
