using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.StudentEnrollments;

public record UnenrollStudentCommand(Guid Id) : IRequest<Result>;
