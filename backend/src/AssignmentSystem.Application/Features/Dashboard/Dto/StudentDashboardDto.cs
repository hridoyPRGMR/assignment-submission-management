namespace AssignmentSystem.Application.Features.Dashboard;

public record StudentDashboardDto(
    int TotalPublishedAssignments,
    int NotSubmittedCount,
    int SubmittedCount,
    int GradedCount);
