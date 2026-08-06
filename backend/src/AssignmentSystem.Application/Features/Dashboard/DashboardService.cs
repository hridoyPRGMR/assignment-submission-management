using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Dashboard;

public class DashboardService(IAppDbContext dbContext) : IDashboardService
{
    public async Task<Result<AdminDashboardDto>> GetAdminDashboardAsync(CancellationToken cancellationToken)
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

    public async Task<Result<StudentDashboardDto>> GetStudentDashboardAsync(Guid studentUserId, CancellationToken cancellationToken)
    {
        var classIds = await dbContext.StudentEnrollments
            .Where(e => e.StudentUserId == studentUserId)
            .Select(e => e.ClassId)
            .ToListAsync(cancellationToken);

        var totalPublishedAssignments = await dbContext.Assignments
            .CountAsync(a => classIds.Contains(a.ClassId) && a.Status == AssignmentStatus.Published, cancellationToken);

        var submittedAssignmentIds = await dbContext.Submissions
            .Where(s => s.StudentUserId == studentUserId)
            .Select(s => s.AssignmentId)
            .ToListAsync(cancellationToken);

        var submittedCount = submittedAssignmentIds.Count;
        var notSubmittedCount = totalPublishedAssignments - submittedCount;

        var gradedCount = await dbContext.Submissions
            .CountAsync(s => s.StudentUserId == studentUserId && s.Status == SubmissionStatus.Graded, cancellationToken);

        var dto = new StudentDashboardDto(totalPublishedAssignments, notSubmittedCount, submittedCount, gradedCount);

        return Result.Success(dto);
    }

    public async Task<Result<TeacherDashboardDto>> GetTeacherDashboardAsync(Guid teacherUserId, CancellationToken cancellationToken)
    {
        var totalAssignments = await dbContext.Assignments
            .CountAsync(a => a.TeacherUserId == teacherUserId, cancellationToken);

        var publishedAssignments = await dbContext.Assignments
            .CountAsync(a => a.TeacherUserId == teacherUserId && a.Status == AssignmentStatus.Published, cancellationToken);

        var draftAssignments = totalAssignments - publishedAssignments;

        var totalSubmissions = await dbContext.Submissions
            .CountAsync(s => s.Assignment.TeacherUserId == teacherUserId, cancellationToken);

        var pendingToGradeCount = await dbContext.Submissions
            .CountAsync(
                s => s.Assignment.TeacherUserId == teacherUserId
                    && (s.Status == SubmissionStatus.Submitted || s.Status == SubmissionStatus.UnderReview),
                cancellationToken);

        var dto = new TeacherDashboardDto(totalAssignments, draftAssignments, publishedAssignments, totalSubmissions, pendingToGradeCount);

        return Result.Success(dto);
    }
}
