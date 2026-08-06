using AssignmentSystem.Domain.Common;
using MediatR;

namespace AssignmentSystem.Application.Features.Dashboard;

public record GetAdminDashboardQuery : IRequest<Result<AdminDashboardDto>>;
