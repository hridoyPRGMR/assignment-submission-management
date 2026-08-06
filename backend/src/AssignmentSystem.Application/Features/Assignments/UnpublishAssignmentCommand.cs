using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Assignments;

public record UnpublishAssignmentCommand(Guid Id, Guid RequestingUserId) : IRequest<Result<AssignmentDto>>;
