using System.Security.Claims;
using AssignmentSystem.Api.Common;
using AssignmentSystem.Application.Features.TeacherAssignments;
using AssignmentSystem.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[Authorize]
[Route("api/v1/teacher-assignments")]
public class TeacherAssignmentsController(ISender sender) : ApiControllerBase(sender)
{
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
            var callerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
            teacherUserId = callerId;
        }

        var result = await Sender.Send(new GetTeacherAssignmentsQuery(teacherUserId, classId, subjectId), cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> AssignTeacher(AssignTeacherCommand command, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> RemoveTeacherAssignment(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new RemoveTeacherAssignmentCommand(id), cancellationToken);
        return HandleResult(result);
    }
}
