using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.StudentEnrollments;

public record EnrollStudentCommand(Guid StudentUserId, Guid ClassId) : IRequest<Result<StudentEnrollmentDto>>;
