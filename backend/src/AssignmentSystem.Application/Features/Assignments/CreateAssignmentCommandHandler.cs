using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Assignments;

public class CreateAssignmentCommandHandler(IAppDbContext dbContext)
    : IRequestHandler<CreateAssignmentCommand, Result<AssignmentDto>>
{
    public async Task<Result<AssignmentDto>> Handle(CreateAssignmentCommand request, CancellationToken cancellationToken)
    {
        var isAssignedToTeach = await dbContext.TeacherAssignments.AnyAsync(
            ta => ta.TeacherUserId == request.TeacherUserId && ta.ClassId == request.ClassId && ta.SubjectId == request.SubjectId,
            cancellationToken);

        if (!isAssignedToTeach)
        {
            return Result.Failure<AssignmentDto>("You are not assigned to teach this subject in this class.");
        }

        var assignmentResult = Domain.Entities.Assignment.Create(
            request.Title,
            request.Description,
            request.ClassId,
            request.SubjectId,
            request.TeacherUserId,
            request.DeadlineUtc,
            request.MaxMarks,
            DateTime.UtcNow);

        if (assignmentResult.IsFailure)
        {
            return Result.Failure<AssignmentDto>(assignmentResult.Error!);
        }

        dbContext.Assignments.Add(assignmentResult.Value);
        await dbContext.SaveChangesAsync(cancellationToken);

        var created = await dbContext.Assignments
            .Include(a => a.Class)
            .Include(a => a.Subject)
            .Include(a => a.TeacherUser)
            .FirstAsync(a => a.Id == assignmentResult.Value.Id, cancellationToken);

        return Result.Success(created.ToDto());
    }
}
