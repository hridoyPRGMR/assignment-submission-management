namespace AssignmentSystem.Application.Features.Assignments;

public record AssignmentDto(
    Guid Id,
    string Title,
    string Description,
    Guid ClassId,
    string ClassName,
    Guid SubjectId,
    string SubjectName,
    Guid TeacherUserId,
    string TeacherFullName,
    DateTime DeadlineUtc,
    int MaxMarks,
    string Status,
    string LifecycleStatus,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
