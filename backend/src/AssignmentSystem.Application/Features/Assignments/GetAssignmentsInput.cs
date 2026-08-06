using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Application.Features.Assignments;

public record GetAssignmentsInput(
    Guid? TeacherUserId,
    Guid? ClassId,
    AssignmentStatus? Status,
    int Page,
    int PageSize);
