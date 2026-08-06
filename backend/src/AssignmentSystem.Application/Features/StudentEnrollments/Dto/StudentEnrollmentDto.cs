namespace AssignmentSystem.Application.Features.StudentEnrollments;

public record StudentEnrollmentDto(
    Guid Id,
    Guid StudentUserId,
    string StudentFullName,
    Guid ClassId,
    string ClassName);
