using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Subjects;

public record GetSubjectByIdQuery(Guid Id) : IRequest<Result<SubjectDto>>;
