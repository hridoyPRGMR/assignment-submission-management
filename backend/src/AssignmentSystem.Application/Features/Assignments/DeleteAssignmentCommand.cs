using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Assignments;

public record DeleteAssignmentCommand(Guid Id, Guid RequestingUserId) : IRequest<Result>;
