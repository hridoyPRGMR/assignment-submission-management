using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.TeacherAssignments;

public record GetTeacherAssignmentsQuery(Guid? TeacherUserId, Guid? ClassId, Guid? SubjectId)
    : IRequest<Result<IReadOnlyList<TeacherAssignmentDto>>>;
