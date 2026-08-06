using AssignmentSystem.Application.Common;
using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Enums;
using MediatR;

namespace AssignmentSystem.Application.Features.Assignments;

public record GetAssignmentsQuery(
    Guid? TeacherUserId,
    Guid? ClassId,
    AssignmentStatus? Status,
    int Page,
    int PageSize) : IRequest<Result<PagedResult<AssignmentDto>>>;
