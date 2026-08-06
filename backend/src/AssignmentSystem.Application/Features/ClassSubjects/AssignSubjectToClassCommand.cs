using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.ClassSubjects;

public record AssignSubjectToClassCommand(Guid ClassId, Guid SubjectId) : IRequest<Result<ClassSubjectDto>>;
