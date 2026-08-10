using System.Security.Claims;
using AssignmentSystem.Api.Common;
using AssignmentSystem.Application.Features.ClassSubjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/v1/class-subjects")]
public class ClassSubjectsController(IClassSubjectService classSubjectService) : ApiControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet]
    public async Task<IActionResult> GetClassSubjects(
        [FromQuery] Guid? classId,
        [FromQuery] Guid? subjectId,
        CancellationToken cancellationToken)
    {
        var result = await classSubjectService.GetForClassAsync(classId, subjectId, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> AssignSubjectToClass(AssignSubjectToClassInput input, CancellationToken cancellationToken)
    {
        var result = await classSubjectService.AssignAsync(input, CurrentUserId, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> RemoveClassSubject(Guid id, CancellationToken cancellationToken)
    {
        var result = await classSubjectService.RemoveAsync(id, CurrentUserId, cancellationToken);
        return HandleResult(result);
    }
}
