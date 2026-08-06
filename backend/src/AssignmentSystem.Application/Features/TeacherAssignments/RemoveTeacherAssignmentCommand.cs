using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.TeacherAssignments;

public record RemoveTeacherAssignmentCommand(Guid Id) : IRequest<Result>;
