using System.Security.Claims;
using AssignmentSystem.Api.Common;
using AssignmentSystem.Application.Features.StudentEnrollments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/v1/student-enrollments")]
public class StudentEnrollmentsController(IStudentEnrollmentService studentEnrollmentService) : ApiControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet]
    public async Task<IActionResult> GetStudentEnrollments(
        [FromQuery] Guid? studentUserId,
        [FromQuery] Guid? classId,
        CancellationToken cancellationToken)
    {
        var result = await studentEnrollmentService.GetForStudentAsync(studentUserId, classId, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> EnrollStudent(EnrollStudentInput input, CancellationToken cancellationToken)
    {
        var result = await studentEnrollmentService.EnrollAsync(input, CurrentUserId, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> UnenrollStudent(Guid id, CancellationToken cancellationToken)
    {
        var result = await studentEnrollmentService.UnenrollAsync(id, CurrentUserId, cancellationToken);
        return HandleResult(result);
    }
}
