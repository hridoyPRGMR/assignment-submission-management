using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.ClassSubjects;

public record RemoveClassSubjectCommand(Guid Id) : IRequest<Result>;
