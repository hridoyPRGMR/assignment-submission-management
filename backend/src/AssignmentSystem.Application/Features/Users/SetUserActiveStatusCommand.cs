using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Users;

public record SetUserActiveStatusCommand(Guid Id, bool IsActive) : IRequest<Result>;
