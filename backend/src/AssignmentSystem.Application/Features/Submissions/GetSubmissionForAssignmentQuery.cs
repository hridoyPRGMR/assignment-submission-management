using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Submissions;

public record GetSubmissionForAssignmentQuery(Guid AssignmentId, Guid StudentUserId) : IRequest<Result<SubmissionDto?>>;
