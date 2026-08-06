using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Classes;

public record GetClassByIdQuery(Guid Id) : IRequest<Result<ClassDto>>;
