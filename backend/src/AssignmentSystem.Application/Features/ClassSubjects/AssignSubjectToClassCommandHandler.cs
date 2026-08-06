using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.ClassSubjects;

public class AssignSubjectToClassCommandHandler(IAppDbContext dbContext)
    : IRequestHandler<AssignSubjectToClassCommand, Result<ClassSubjectDto>>
{
    public async Task<Result<ClassSubjectDto>> Handle(AssignSubjectToClassCommand request, CancellationToken cancellationToken)
    {
        var classEntity = await dbContext.Classes.FirstOrDefaultAsync(c => c.Id == request.ClassId, cancellationToken);
        if (classEntity is null)
        {
            return Result.Failure<ClassSubjectDto>("Class not found.");
        }

        var subject = await dbContext.Subjects.FirstOrDefaultAsync(s => s.Id == request.SubjectId, cancellationToken);
        if (subject is null)
        {
            return Result.Failure<ClassSubjectDto>("Subject not found.");
        }

        var alreadyLinked = await dbContext.ClassSubjects
            .AnyAsync(cs => cs.ClassId == request.ClassId && cs.SubjectId == request.SubjectId, cancellationToken);

        if (alreadyLinked)
        {
            return Result.Failure<ClassSubjectDto>("This subject is already associated with the class.");
        }

        var classSubject = ClassSubject.Create(request.ClassId, request.SubjectId);
        dbContext.ClassSubjects.Add(classSubject);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new ClassSubjectDto(classSubject.Id, classEntity.Id, classEntity.Name, subject.Id, subject.Name));
    }
}
