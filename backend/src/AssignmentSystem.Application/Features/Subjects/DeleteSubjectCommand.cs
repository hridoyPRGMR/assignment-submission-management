using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Subjects;

public record DeleteSubjectCommand(Guid Id) : IRequest<Result>;
