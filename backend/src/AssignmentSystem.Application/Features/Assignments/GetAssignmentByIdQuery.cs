using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Assignments;

public record GetAssignmentByIdQuery(Guid Id) : IRequest<Result<AssignmentDto>>;
