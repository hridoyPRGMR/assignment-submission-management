using System.Security.Claims;
using AssignmentSystem.Api.Common;
using AssignmentSystem.Application.Features.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[Authorize]
[Route("api/v1/dashboard")]
public class DashboardController(IDashboardService dashboardService) : ApiControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet("admin")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> GetAdminDashboard(CancellationToken cancellationToken)
    {
        var result = await dashboardService.GetAdminDashboardAsync(cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("teacher")]
    [Authorize(Policy = AuthorizationPolicies.TeacherOnly)]
    public async Task<IActionResult> GetTeacherDashboard(CancellationToken cancellationToken)
    {
        var result = await dashboardService.GetTeacherDashboardAsync(CurrentUserId, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("student")]
    [Authorize(Policy = AuthorizationPolicies.StudentOnly)]
    public async Task<IActionResult> GetStudentDashboard(CancellationToken cancellationToken)
    {
        var result = await dashboardService.GetStudentDashboardAsync(CurrentUserId, cancellationToken);
        return HandleResult(result);
    }
}
