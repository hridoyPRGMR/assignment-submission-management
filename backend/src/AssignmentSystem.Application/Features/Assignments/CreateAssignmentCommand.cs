using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Assignments;

public record CreateAssignmentCommand(
    Guid TeacherUserId,
    string Title,
    string Description,
    Guid ClassId,
    Guid SubjectId,
    DateTime DeadlineUtc,
    int MaxMarks) : IRequest<Result<AssignmentDto>>;
