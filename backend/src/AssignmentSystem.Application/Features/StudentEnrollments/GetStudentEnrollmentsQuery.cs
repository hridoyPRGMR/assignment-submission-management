using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.StudentEnrollments;

public record GetStudentEnrollmentsQuery(Guid? StudentUserId, Guid? ClassId)
    : IRequest<Result<IReadOnlyList<StudentEnrollmentDto>>>;
