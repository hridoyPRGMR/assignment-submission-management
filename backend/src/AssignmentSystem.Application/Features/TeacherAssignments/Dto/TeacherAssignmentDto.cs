namespace AssignmentSystem.Application.Features.TeacherAssignments;

public record TeacherAssignmentDto(
    Guid Id,
    Guid TeacherUserId,
    string TeacherFullName,
    Guid ClassId,
    string ClassName,
    Guid SubjectId,
    string SubjectName);
