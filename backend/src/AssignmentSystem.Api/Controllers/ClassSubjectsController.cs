using AssignmentSystem.Api.Common;
using AssignmentSystem.Application.Features.ClassSubjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/v1/class-subjects")]
public class ClassSubjectsController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet]
    public async Task<IActionResult> GetClassSubjects(
        [FromQuery] Guid? classId,
        [FromQuery] Guid? subjectId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetClassSubjectsQuery(classId, subjectId), cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> AssignSubjectToClass(AssignSubjectToClassCommand command, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> RemoveClassSubject(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new RemoveClassSubjectCommand(id), cancellationToken);
        return HandleResult(result);
    }
}
