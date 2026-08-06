using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Subjects;

public record CreateSubjectCommand(string Name, string? Description) : IRequest<Result<SubjectDto>>;
