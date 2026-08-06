using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Classes;

public record DeleteClassCommand(Guid Id) : IRequest<Result>;
