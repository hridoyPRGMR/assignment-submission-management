namespace AssignmentSystem.Application.Features.Dashboard;

public record AdminDashboardDto(
    int TotalUsers,
    int TotalTeachers,
    int TotalStudents,
    int TotalClasses,
    int TotalSubjects,
    int TotalAssignments,
    int PublishedAssignments,
    int DraftAssignments,
    int TotalSubmissions,
    int GradedSubmissions);
