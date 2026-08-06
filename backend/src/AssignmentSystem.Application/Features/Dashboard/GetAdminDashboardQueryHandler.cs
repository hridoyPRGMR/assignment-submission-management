using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Dashboard;

public class GetAdminDashboardQueryHandler(IAppDbContext dbContext)
    : IRequestHandler<GetAdminDashboardQuery, Result<AdminDashboardDto>>
{
    public async Task<Result<AdminDashboardDto>> Handle(GetAdminDashboardQuery request, CancellationToken cancellationToken)
    {
        var totalUsers = await dbContext.Users.CountAsync(cancellationToken);
        var totalTeachers = await dbContext.Users.CountAsync(u => u.Role == UserRole.Teacher, cancellationToken);
        var totalStudents = await dbContext.Users.CountAsync(u => u.Role == UserRole.Student, cancellationToken);
        var totalClasses = await dbContext.Classes.CountAsync(cancellationToken);
        var totalSubjects = await dbContext.Subjects.CountAsync(cancellationToken);
        var totalAssignments = await dbContext.Assignments.CountAsync(cancellationToken);
        var publishedAssignments = await dbContext.Assignments.CountAsync(a => a.Status == AssignmentStatus.Published, cancellationToken);
        var draftAssignments = totalAssignments - publishedAssignments;
        var totalSubmissions = await dbContext.Submissions.CountAsync(cancellationToken);
        var gradedSubmissions = await dbContext.Submissions.CountAsync(s => s.Status == SubmissionStatus.Graded, cancellationToken);

        var dto = new AdminDashboardDto(
            totalUsers,
            totalTeachers,
            totalStudents,
            totalClasses,
            totalSubjects,
            totalAssignments,
            publishedAssignments,
            draftAssignments,
            totalSubmissions,
            gradedSubmissions);

        return Result.Success(dto);
    }
}
