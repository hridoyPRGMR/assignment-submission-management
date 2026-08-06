namespace AssignmentSystem.Application.Features.Dashboard;

public record TeacherDashboardDto(
    int TotalAssignments,
    int DraftAssignments,
    int PublishedAssignments,
    int TotalSubmissions,
    int PendingToGradeCount);
