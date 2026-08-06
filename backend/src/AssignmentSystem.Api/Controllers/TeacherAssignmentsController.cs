using System.Security.Claims;
using AssignmentSystem.Api.Common;
using AssignmentSystem.Application.Features.TeacherAssignments;
using AssignmentSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[Authorize]
[Route("api/v1/teacher-assignments")]
public class TeacherAssignmentsController(ITeacherAssignmentService teacherAssignmentService) : ApiControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet]
    public async Task<IActionResult> GetTeacherAssignments(
        [FromQuery] Guid? teacherUserId,
        [FromQuery] Guid? classId,
        [FromQuery] Guid? subjectId,
        CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole(UserRole.Admin.ToString());

        if (!isAdmin)
        {
            teacherUserId = CurrentUserId;
        }

        var result = await teacherAssignmentService.GetForTeacherAsync(teacherUserId, classId, subjectId, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> AssignTeacher(AssignTeacherInput input, CancellationToken cancellationToken)
    {
        var result = await teacherAssignmentService.AssignAsync(input, CurrentUserId, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> RemoveTeacherAssignment(Guid id, CancellationToken cancellationToken)
    {
        var result = await teacherAssignmentService.RemoveAsync(id, cancellationToken);
        return HandleResult(result);
    }
}
