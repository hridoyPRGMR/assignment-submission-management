using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Subjects;

public record UpdateSubjectCommand(Guid Id, string Name, string? Description) : IRequest<Result<SubjectDto>>;
