using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Submissions;

public record CreateSubmissionCommand(Guid StudentUserId, Guid AssignmentId, string Content, string? FileUrl)
    : IRequest<Result<SubmissionDto>>;
