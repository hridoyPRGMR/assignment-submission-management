using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Submissions;

public record GetMySubmissionsQuery(Guid StudentUserId) : IRequest<Result<IReadOnlyList<SubmissionDto>>>;
