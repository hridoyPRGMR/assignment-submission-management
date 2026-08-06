using AssignmentSystem.Api.Common;
using AssignmentSystem.Application.Features.StudentEnrollments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/v1/student-enrollments")]
public class StudentEnrollmentsController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet]
    public async Task<IActionResult> GetStudentEnrollments(
        [FromQuery] Guid? studentUserId,
        [FromQuery] Guid? classId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetStudentEnrollmentsQuery(studentUserId, classId), cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> EnrollStudent(EnrollStudentCommand command, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> UnenrollStudent(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new UnenrollStudentCommand(id), cancellationToken);
        return HandleResult(result);
    }
}
