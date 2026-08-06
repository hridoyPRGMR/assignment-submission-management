using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.TeacherAssignments;

public class TeacherAssignmentService(IAppDbContext dbContext, IValidator<AssignTeacherInput> assignValidator)
    : ITeacherAssignmentService
{
    public async Task<Result<TeacherAssignmentDto>> AssignAsync(AssignTeacherInput input, CancellationToken cancellationToken)
    {
        var validation = await assignValidator.ValidateAsync(input, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure<TeacherAssignmentDto>(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage)));
        }

        var teacher = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == input.TeacherUserId, cancellationToken);
        if (teacher is null || teacher.Role != UserRole.Teacher || !teacher.IsActive)
        {
            return Result.Failure<TeacherAssignmentDto>("Teacher not found or is not an active teacher account.");
        }

        var classEntity = await dbContext.Classes.FirstOrDefaultAsync(c => c.Id == input.ClassId, cancellationToken);
        if (classEntity is null)
        {
            return Result.Failure<TeacherAssignmentDto>("Class not found.");
        }

        var subject = await dbContext.Subjects.FirstOrDefaultAsync(s => s.Id == input.SubjectId, cancellationToken);
        if (subject is null)
        {
            return Result.Failure<TeacherAssignmentDto>("Subject not found.");
        }

        var subjectTaughtInClass = await dbContext.ClassSubjects
            .AnyAsync(cs => cs.ClassId == input.ClassId && cs.SubjectId == input.SubjectId, cancellationToken);

        if (!subjectTaughtInClass)
        {
            return Result.Failure<TeacherAssignmentDto>("This subject is not associated with the given class.");
        }

        var alreadyAssigned = await dbContext.TeacherAssignments.AnyAsync(
            ta => ta.TeacherUserId == input.TeacherUserId && ta.ClassId == input.ClassId && ta.SubjectId == input.SubjectId,
            cancellationToken);

        if (alreadyAssigned)
        {
            return Result.Failure<TeacherAssignmentDto>("This teacher is already assigned to this class and subject.");
        }

        var teacherAssignment = TeacherAssignment.Create(input.TeacherUserId, input.ClassId, input.SubjectId);
        dbContext.TeacherAssignments.Add(teacherAssignment);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new TeacherAssignmentDto(
            teacherAssignment.Id,
            teacher.Id,
            teacher.FullName,
            classEntity.Id,
            classEntity.Name,
            subject.Id,
            subject.Name));
    }

    public async Task<Result<IReadOnlyList<TeacherAssignmentDto>>> GetForTeacherAsync(Guid? teacherUserId, Guid? classId, Guid? subjectId, CancellationToken cancellationToken)
    {
        var query = dbContext.TeacherAssignments
            .AsNoTracking()
            .Include(ta => ta.TeacherUser)
            .Include(ta => ta.Class)
            .Include(ta => ta.Subject)
            .AsQueryable();

        if (teacherUserId.HasValue)
        {
            query = query.Where(ta => ta.TeacherUserId == teacherUserId.Value);
        }

        if (classId.HasValue)
        {
            query = query.Where(ta => ta.ClassId == classId.Value);
        }

        if (subjectId.HasValue)
        {
            query = query.Where(ta => ta.SubjectId == subjectId.Value);
        }

        var items = await query
            .Select(ta => new TeacherAssignmentDto(
                ta.Id, ta.TeacherUserId, ta.TeacherUser.FullName, ta.ClassId, ta.Class.Name, ta.SubjectId, ta.Subject.Name))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<TeacherAssignmentDto>>(items);
    }

    public async Task<Result> RemoveAsync(Guid id, CancellationToken cancellationToken)
    {
        var teacherAssignment = await dbContext.TeacherAssignments.FirstOrDefaultAsync(ta => ta.Id == id, cancellationToken);

        if (teacherAssignment is null)
        {
            return Result.Failure("Teacher assignment not found.");
        }

        dbContext.TeacherAssignments.Remove(teacherAssignment);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
