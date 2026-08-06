using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Dashboard;

public record GetStudentDashboardQuery(Guid StudentUserId) : IRequest<Result<StudentDashboardDto>>;
