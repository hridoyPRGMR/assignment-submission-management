using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Submissions;

public record UpdateSubmissionCommand(Guid Id, Guid RequestingStudentUserId, string Content, string? FileUrl)
    : IRequest<Result<SubmissionDto>>;
