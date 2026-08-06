namespace AssignmentSystem.Application.Features.Assignments;

public record UpdateAssignmentInput(
    Guid Id,
    Guid RequestingUserId,
    string Title,
    string Description,
    DateTime DeadlineUtc,
    int MaxMarks);
