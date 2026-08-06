using AssignmentSystem.Application.Common;
using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Assignments;

public record GetPublishedAssignmentsForStudentQuery(Guid StudentUserId, int Page, int PageSize)
    : IRequest<Result<PagedResult<AssignmentDto>>>;
