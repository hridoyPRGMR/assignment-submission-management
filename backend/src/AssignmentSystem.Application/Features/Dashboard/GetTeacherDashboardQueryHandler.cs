using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Dashboard;

public class GetTeacherDashboardQueryHandler(IAppDbContext dbContext)
    : IRequestHandler<GetTeacherDashboardQuery, Result<TeacherDashboardDto>>
{
    public async Task<Result<TeacherDashboardDto>> Handle(GetTeacherDashboardQuery request, CancellationToken cancellationToken)
    {
        var totalAssignments = await dbContext.Assignments
            .CountAsync(a => a.TeacherUserId == request.TeacherUserId, cancellationToken);

        var publishedAssignments = await dbContext.Assignments
            .CountAsync(a => a.TeacherUserId == request.TeacherUserId && a.Status == AssignmentStatus.Published, cancellationToken);

        var draftAssignments = totalAssignments - publishedAssignments;

        var totalSubmissions = await dbContext.Submissions
            .CountAsync(s => s.Assignment.TeacherUserId == request.TeacherUserId, cancellationToken);

        var pendingToGradeCount = await dbContext.Submissions
            .CountAsync(
                s => s.Assignment.TeacherUserId == request.TeacherUserId
                    && (s.Status == SubmissionStatus.Submitted || s.Status == SubmissionStatus.UnderReview),
                cancellationToken);

        var dto = new TeacherDashboardDto(totalAssignments, draftAssignments, publishedAssignments, totalSubmissions, pendingToGradeCount);

        return Result.Success(dto);
    }
}
