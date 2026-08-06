using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.ClassSubjects;

public record GetClassSubjectsQuery(Guid? ClassId, Guid? SubjectId) : IRequest<Result<IReadOnlyList<ClassSubjectDto>>>;
