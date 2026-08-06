using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Submissions;

public record GradeSubmissionCommand(Guid SubmissionId, Guid RequestingTeacherUserId, int Marks, string Feedback)
    : IRequest<Result<SubmissionDto>>;
