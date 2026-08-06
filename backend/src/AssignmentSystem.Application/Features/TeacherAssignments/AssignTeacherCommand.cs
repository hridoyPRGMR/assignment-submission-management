using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.TeacherAssignments;

public record AssignTeacherCommand(Guid TeacherUserId, Guid ClassId, Guid SubjectId) : IRequest<Result<TeacherAssignmentDto>>;
