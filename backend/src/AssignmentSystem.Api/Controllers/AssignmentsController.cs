using System.Security.Claims;
using AssignmentSystem.Api.Common;
using AssignmentSystem.Application.Features.Assignments;
using AssignmentSystem.Application.Features.StudentEnrollments;
using AssignmentSystem.Application.Features.Submissions;
using AssignmentSystem.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[Authorize]
public class AssignmentsController(ISender sender, IAuthorizationService authorizationService) : ApiControllerBase(sender)
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.TeacherOnly)]
    public async Task<IActionResult> GetMyAssignments(
        [FromQuery] AssignmentStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetAssignmentsQuery(CurrentUserId, null, status, page, pageSize), cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("admin")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> GetAllAssignmentsForAdmin(
        [FromQuery] Guid? classId,
        [FromQuery] AssignmentStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetAssignmentsQuery(null, classId, status, page, pageSize), cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("student")]
    [Authorize(Policy = AuthorizationPolicies.StudentOnly)]
    public async Task<IActionResult> GetPublishedAssignmentsForStudent(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetPublishedAssignmentsForStudentQuery(CurrentUserId, page, pageSize), cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAssignmentById(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetAssignmentByIdQuery(id), cancellationToken);
        if (result.IsFailure)
        {
            return HandleResult(result);
        }

        if (User.IsInRole(UserRole.Teacher.ToString()))
        {
            var authResult = await authorizationService.AuthorizeAsync(User, result.Value, AuthorizationPolicies.AssignmentOwner);
            return authResult.Succeeded ? Ok(result.Value) : Forbid();
        }

        if (User.IsInRole(UserRole.Student.ToString()))
        {
            if (result.Value.Status != "Published")
            {
                return NotFound();
            }

            var isEnrolled = await Sender.Send(new IsStudentEnrolledQuery(CurrentUserId, result.Value.ClassId), cancellationToken);
            return isEnrolled.IsSuccess && isEnrolled.Value ? Ok(result.Value) : Forbid();
        }

        return Forbid();
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.TeacherOnly)]
    public async Task<IActionResult> CreateAssignment(CreateAssignmentRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateAssignmentCommand(
            CurrentUserId, request.Title, request.Description, request.ClassId, request.SubjectId, request.DeadlineUtc, request.MaxMarks);

        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.TeacherOnly)]
    public async Task<IActionResult> UpdateAssignment(Guid id, UpdateAssignmentRequest request, CancellationToken cancellationToken)
    {
        var authCheck = await AuthorizeAgainstExistingAssignment(id, cancellationToken);
        if (authCheck is not null)
        {
            return authCheck;
        }

        var command = new UpdateAssignmentCommand(
            id, CurrentUserId, request.Title, request.Description, request.DeadlineUtc, request.MaxMarks);

        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/publish")]
    [Authorize(Policy = AuthorizationPolicies.TeacherOnly)]
    public async Task<IActionResult> Publish(Guid id, CancellationToken cancellationToken)
    {
        var authCheck = await AuthorizeAgainstExistingAssignment(id, cancellationToken);
        if (authCheck is not null)
        {
            return authCheck;
        }

        var result = await Sender.Send(new PublishAssignmentCommand(id, CurrentUserId), cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/unpublish")]
    [Authorize(Policy = AuthorizationPolicies.TeacherOnly)]
    public async Task<IActionResult> Unpublish(Guid id, CancellationToken cancellationToken)
    {
        var authCheck = await AuthorizeAgainstExistingAssignment(id, cancellationToken);
        if (authCheck is not null)
        {
            return authCheck;
        }

        var result = await Sender.Send(new UnpublishAssignmentCommand(id, CurrentUserId), cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.TeacherOnly)]
    public async Task<IActionResult> DeleteAssignment(Guid id, CancellationToken cancellationToken)
    {
        var authCheck = await AuthorizeAgainstExistingAssignment(id, cancellationToken);
        if (authCheck is not null)
        {
            return authCheck;
        }

        var result = await Sender.Send(new DeleteAssignmentCommand(id, CurrentUserId), cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}/submissions")]
    [Authorize(Policy = AuthorizationPolicies.TeacherOrAdmin)]
    public async Task<IActionResult> GetSubmissionsForAssignment(Guid id, CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole(UserRole.Admin.ToString());
        var result = await Sender.Send(new GetSubmissionsForAssignmentQuery(id, CurrentUserId, isAdmin), cancellationToken);
        return HandleResult(result);
    }

    private async Task<IActionResult?> AuthorizeAgainstExistingAssignment(Guid id, CancellationToken cancellationToken)
    {
        var existing = await Sender.Send(new GetAssignmentByIdQuery(id), cancellationToken);
        if (existing.IsFailure)
        {
            return NotFound();
        }

        var authResult = await authorizationService.AuthorizeAsync(User, existing.Value, AuthorizationPolicies.AssignmentOwner);
        return authResult.Succeeded ? null : Forbid();
    }
}

public record CreateAssignmentRequest(
    string Title, string Description, Guid ClassId, Guid SubjectId, DateTime DeadlineUtc, int MaxMarks);

public record UpdateAssignmentRequest(string Title, string Description, DateTime DeadlineUtc, int MaxMarks);
