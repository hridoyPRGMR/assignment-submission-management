using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Submissions;

public record ReturnSubmissionForRevisionCommand(Guid SubmissionId, Guid RequestingTeacherUserId, string Feedback)
    : IRequest<Result<SubmissionDto>>;
