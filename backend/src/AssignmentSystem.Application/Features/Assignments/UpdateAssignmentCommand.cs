using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Assignments;

public record UpdateAssignmentCommand(
    Guid Id,
    Guid RequestingUserId,
    string Title,
    string Description,
    DateTime DeadlineUtc,
    int MaxMarks) : IRequest<Result<AssignmentDto>>;
