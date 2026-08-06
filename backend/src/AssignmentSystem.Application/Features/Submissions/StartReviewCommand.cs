using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Submissions;

public record StartReviewCommand(Guid SubmissionId, Guid RequestingTeacherUserId) : IRequest<Result<SubmissionDto>>;
