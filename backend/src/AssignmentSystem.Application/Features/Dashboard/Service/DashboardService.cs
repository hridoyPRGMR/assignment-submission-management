using AssignmentSystem.Application.Abstractions.Persistence;
using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Application.Features.Dashboard;

public class DashboardService(
    IAssignmentRepository assignmentRepository,
    IClassRepository classRepository,
    IStudentEnrollmentRepository studentEnrollmentRepository,
    ISubjectRepository subjectRepository,
    ISubmissionRepository submissionRepository,
    IUserRepository userRepository) : IDashboardService
{
    public async Task<Result<AdminDashboardDto>> GetAdminDashboardAsync(CancellationToken cancellationToken)
    {
        var totalUsers = await userRepository.Query().CountAsync(cancellationToken);
        var totalTeachers = await userRepository.Query().CountAsync(u => u.Role == UserRole.Teacher, cancellationToken);
        var totalStudents = await userRepository.Query().CountAsync(u => u.Role == UserRole.Student, cancellationToken);
        var totalClasses = await classRepository.Query().CountAsync(cancellationToken);
        var totalSubjects = await subjectRepository.Query().CountAsync(cancellationToken);
        var totalAssignments = await assignmentRepository.Query().CountAsync(cancellationToken);
        var publishedAssignments = await assignmentRepository.Query().CountAsync(a => a.Status == AssignmentStatus.Published, cancellationToken);
        var draftAssignments = totalAssignments - publishedAssignments;
        var totalSubmissions = await submissionRepository.Query().CountAsync(cancellationToken);
        var gradedSubmissions = await submissionRepository.Query().CountAsync(s => s.Status == SubmissionStatus.Graded, cancellationToken);

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
        var classIds = await studentEnrollmentRepository.Query()
            .Where(e => e.StudentUserId == studentUserId)
            .Select(e => e.ClassId)
            .ToListAsync(cancellationToken);

        var totalPublishedAssignments = await assignmentRepository.Query()
            .CountAsync(a => classIds.Contains(a.ClassId) && a.Status == AssignmentStatus.Published, cancellationToken);

        var submittedAssignmentIds = await submissionRepository.Query()
            .Where(s => s.StudentUserId == studentUserId)
            .Select(s => s.AssignmentId)
            .ToListAsync(cancellationToken);

        var submittedCount = submittedAssignmentIds.Count;
        var notSubmittedCount = totalPublishedAssignments - submittedCount;

        var gradedCount = await submissionRepository.Query()
            .CountAsync(s => s.StudentUserId == studentUserId && s.Status == SubmissionStatus.Graded, cancellationToken);

        var dto = new StudentDashboardDto(totalPublishedAssignments, notSubmittedCount, submittedCount, gradedCount);

        return Result.Success(dto);
    }

    public async Task<Result<TeacherDashboardDto>> GetTeacherDashboardAsync(Guid teacherUserId, CancellationToken cancellationToken)
    {
        var totalAssignments = await assignmentRepository.Query()
            .CountAsync(a => a.TeacherUserId == teacherUserId, cancellationToken);

        var publishedAssignments = await assignmentRepository.Query()
            .CountAsync(a => a.TeacherUserId == teacherUserId && a.Status == AssignmentStatus.Published, cancellationToken);

        var draftAssignments = totalAssignments - publishedAssignments;

        var totalSubmissions = await submissionRepository.Query()
            .CountAsync(s => s.Assignment.TeacherUserId == teacherUserId, cancellationToken);

        var pendingToGradeCount = await submissionRepository.Query()
            .CountAsync(
                s => s.Assignment.TeacherUserId == teacherUserId
                    && (s.Status == SubmissionStatus.Submitted || s.Status == SubmissionStatus.UnderReview),
                cancellationToken);

        var dto = new TeacherDashboardDto(totalAssignments, draftAssignments, publishedAssignments, totalSubmissions, pendingToGradeCount);

        return Result.Success(dto);
    }
}
