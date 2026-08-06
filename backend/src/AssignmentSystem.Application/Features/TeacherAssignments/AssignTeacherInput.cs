namespace AssignmentSystem.Application.Features.TeacherAssignments;

public record AssignTeacherInput(Guid TeacherUserId, Guid ClassId, Guid SubjectId);
