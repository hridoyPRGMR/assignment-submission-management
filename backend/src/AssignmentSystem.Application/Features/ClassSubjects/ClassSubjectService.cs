using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.ClassSubjects;

public class ClassSubjectService(
    IAppDbContext dbContext,
    IValidator<AssignSubjectToClassInput> assignValidator) : IClassSubjectService
{
    public async Task<Result<ClassSubjectDto>> AssignAsync(AssignSubjectToClassInput input, CancellationToken cancellationToken)
    {
        var validation = await assignValidator.ValidateAsync(input, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure<ClassSubjectDto>(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage)));
        }

        var classEntity = await dbContext.Classes.FirstOrDefaultAsync(c => c.Id == input.ClassId, cancellationToken);
        if (classEntity is null)
        {
            return Result.Failure<ClassSubjectDto>("Class not found.");
        }

        var subject = await dbContext.Subjects.FirstOrDefaultAsync(s => s.Id == input.SubjectId, cancellationToken);
        if (subject is null)
        {
            return Result.Failure<ClassSubjectDto>("Subject not found.");
        }

        var alreadyLinked = await dbContext.ClassSubjects
            .AnyAsync(cs => cs.ClassId == input.ClassId && cs.SubjectId == input.SubjectId, cancellationToken);

        if (alreadyLinked)
        {
            return Result.Failure<ClassSubjectDto>("This subject is already associated with the class.");
        }

        var classSubject = ClassSubject.Create(input.ClassId, input.SubjectId);
        dbContext.ClassSubjects.Add(classSubject);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new ClassSubjectDto(classSubject.Id, classEntity.Id, classEntity.Name, subject.Id, subject.Name));
    }

    public async Task<Result<IReadOnlyList<ClassSubjectDto>>> GetForClassAsync(Guid? classId, Guid? subjectId, CancellationToken cancellationToken)
    {
        var query = dbContext.ClassSubjects
            .AsNoTracking()
            .Include(cs => cs.Class)
            .Include(cs => cs.Subject)
            .AsQueryable();

        if (classId.HasValue)
        {
            query = query.Where(cs => cs.ClassId == classId.Value);
        }

        if (subjectId.HasValue)
        {
            query = query.Where(cs => cs.SubjectId == subjectId.Value);
        }

        var items = await query
            .Select(cs => new ClassSubjectDto(cs.Id, cs.ClassId, cs.Class.Name, cs.SubjectId, cs.Subject.Name))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<ClassSubjectDto>>(items);
    }

    public async Task<Result> RemoveAsync(Guid id, CancellationToken cancellationToken)
    {
        var classSubject = await dbContext.ClassSubjects.FirstOrDefaultAsync(cs => cs.Id == id, cancellationToken);

        if (classSubject is null)
        {
            return Result.Failure("Class-subject association not found.");
        }

        var hasTeacherAssignments = await dbContext.TeacherAssignments.AnyAsync(
            ta => ta.ClassId == classSubject.ClassId && ta.SubjectId == classSubject.SubjectId, cancellationToken);

        if (hasTeacherAssignments)
        {
            return Result.Failure("Cannot remove this association while teachers are assigned to it.");
        }

        dbContext.ClassSubjects.Remove(classSubject);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
