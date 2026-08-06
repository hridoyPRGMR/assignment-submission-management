using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Classes;

public record CreateClassCommand(string Name, string? Description) : IRequest<Result<ClassDto>>;
