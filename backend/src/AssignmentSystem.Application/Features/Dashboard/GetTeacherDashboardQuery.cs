using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Dashboard;

public record GetTeacherDashboardQuery(Guid TeacherUserId) : IRequest<Result<TeacherDashboardDto>>;
