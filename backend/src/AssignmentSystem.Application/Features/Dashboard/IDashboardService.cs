using AssignmentSystem.Domain.Common;

namespace AssignmentSystem.Application.Features.Dashboard;

public interface IDashboardService
{
    Task<Result<AdminDashboardDto>> GetAdminDashboardAsync(CancellationToken cancellationToken);

    Task<Result<StudentDashboardDto>> GetStudentDashboardAsync(Guid studentUserId, CancellationToken cancellationToken);

    Task<Result<TeacherDashboardDto>> GetTeacherDashboardAsync(Guid teacherUserId, CancellationToken cancellationToken);
}
