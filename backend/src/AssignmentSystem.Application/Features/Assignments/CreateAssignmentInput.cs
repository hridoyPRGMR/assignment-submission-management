namespace AssignmentSystem.Application.Features.Assignments;

public record CreateAssignmentInput(
    Guid TeacherUserId,
    string Title,
    string Description,
    Guid ClassId,
    Guid SubjectId,
    DateTime DeadlineUtc,
    int MaxMarks);
