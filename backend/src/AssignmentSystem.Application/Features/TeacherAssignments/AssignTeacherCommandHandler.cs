using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.TeacherAssignments;

public class AssignTeacherCommandHandler(IAppDbContext dbContext)
    : IRequestHandler<AssignTeacherCommand, Result<TeacherAssignmentDto>>
{
    public async Task<Result<TeacherAssignmentDto>> Handle(AssignTeacherCommand request, CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.TeacherUserId, cancellationToken);
        if (teacher is null || teacher.Role != UserRole.Teacher || !teacher.IsActive)
        {
            return Result.Failure<TeacherAssignmentDto>("Teacher not found or is not an active teacher account.");
        }

        var classEntity = await dbContext.Classes.FirstOrDefaultAsync(c => c.Id == request.ClassId, cancellationToken);
        if (classEntity is null)
        {
            return Result.Failure<TeacherAssignmentDto>("Class not found.");
        }

        var subject = await dbContext.Subjects.FirstOrDefaultAsync(s => s.Id == request.SubjectId, cancellationToken);
        if (subject is null)
        {
            return Result.Failure<TeacherAssignmentDto>("Subject not found.");
        }

        var subjectTaughtInClass = await dbContext.ClassSubjects
            .AnyAsync(cs => cs.ClassId == request.ClassId && cs.SubjectId == request.SubjectId, cancellationToken);

        if (!subjectTaughtInClass)
        {
            return Result.Failure<TeacherAssignmentDto>("This subject is not associated with the given class.");
        }

        var alreadyAssigned = await dbContext.TeacherAssignments.AnyAsync(
            ta => ta.TeacherUserId == request.TeacherUserId && ta.ClassId == request.ClassId && ta.SubjectId == request.SubjectId,
            cancellationToken);

        if (alreadyAssigned)
        {
            return Result.Failure<TeacherAssignmentDto>("This teacher is already assigned to this class and subject.");
        }

        var teacherAssignment = TeacherAssignment.Create(request.TeacherUserId, request.ClassId, request.SubjectId);
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
}
