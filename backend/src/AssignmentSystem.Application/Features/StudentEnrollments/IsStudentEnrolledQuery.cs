using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.StudentEnrollments;

public record IsStudentEnrolledQuery(Guid StudentUserId, Guid ClassId) : IRequest<Result<bool>>;
