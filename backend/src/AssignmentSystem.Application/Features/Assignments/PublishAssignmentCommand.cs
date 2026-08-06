using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Assignments;

public record PublishAssignmentCommand(Guid Id, Guid RequestingUserId) : IRequest<Result<AssignmentDto>>;
