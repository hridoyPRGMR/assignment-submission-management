using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Submissions;

public record GetSubmissionsForAssignmentQuery(Guid AssignmentId, Guid RequestingUserId, bool IsAdmin)
    : IRequest<Result<IReadOnlyList<SubmissionDto>>>;
