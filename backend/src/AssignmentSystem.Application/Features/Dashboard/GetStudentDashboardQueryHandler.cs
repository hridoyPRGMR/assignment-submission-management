using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Dashboard;

public class GetStudentDashboardQueryHandler(IAppDbContext dbContext)
    : IRequestHandler<GetStudentDashboardQuery, Result<StudentDashboardDto>>
{
    public async Task<Result<StudentDashboardDto>> Handle(GetStudentDashboardQuery request, CancellationToken cancellationToken)
    {
        var classIds = await dbContext.StudentEnrollments
            .Where(e => e.StudentUserId == request.StudentUserId)
            .Select(e => e.ClassId)
            .ToListAsync(cancellationToken);

        var totalPublishedAssignments = await dbContext.Assignments
            .CountAsync(a => classIds.Contains(a.ClassId) && a.Status == AssignmentStatus.Published, cancellationToken);

        var submittedAssignmentIds = await dbContext.Submissions
            .Where(s => s.StudentUserId == request.StudentUserId)
            .Select(s => s.AssignmentId)
            .ToListAsync(cancellationToken);

        var submittedCount = submittedAssignmentIds.Count;
        var notSubmittedCount = totalPublishedAssignments - submittedCount;

        var gradedCount = await dbContext.Submissions
            .CountAsync(s => s.StudentUserId == request.StudentUserId && s.Status == SubmissionStatus.Graded, cancellationToken);

        var dto = new StudentDashboardDto(totalPublishedAssignments, notSubmittedCount, submittedCount, gradedCount);

        return Result.Success(dto);
    }
}
