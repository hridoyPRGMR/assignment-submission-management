using System.Security.Claims;
using AssignmentSystem.Api.Common;
using AssignmentSystem.Application.Features.Submissions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[Authorize]
public class SubmissionsController(ISender sender) : ApiControllerBase(sender)
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.StudentOnly)]
    public async Task<IActionResult> GetMySubmissions(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetMySubmissionsQuery(CurrentUserId), cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("for-assignment/{assignmentId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.StudentOnly)]
    public async Task<IActionResult> GetMySubmissionForAssignment(Guid assignmentId, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetSubmissionForAssignmentQuery(assignmentId, CurrentUserId), cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.StudentOnly)]
    public async Task<IActionResult> CreateSubmission(CreateSubmissionRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateSubmissionCommand(CurrentUserId, request.AssignmentId, request.Content, request.FileUrl);
        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.StudentOnly)]
    public async Task<IActionResult> UpdateSubmission(Guid id, UpdateSubmissionRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateSubmissionCommand(id, CurrentUserId, request.Content, request.FileUrl);
        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/start-review")]
    [Authorize(Policy = AuthorizationPolicies.TeacherOnly)]
    public async Task<IActionResult> StartReview(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new StartReviewCommand(id, CurrentUserId), cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/grade")]
    [Authorize(Policy = AuthorizationPolicies.TeacherOnly)]
    public async Task<IActionResult> Grade(Guid id, GradeSubmissionRequest request, CancellationToken cancellationToken)
    {
        var command = new GradeSubmissionCommand(id, CurrentUserId, request.Marks, request.Feedback);
        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/return-for-revision")]
    [Authorize(Policy = AuthorizationPolicies.TeacherOnly)]
    public async Task<IActionResult> ReturnForRevision(Guid id, ReturnForRevisionRequest request, CancellationToken cancellationToken)
    {
        var command = new ReturnSubmissionForRevisionCommand(id, CurrentUserId, request.Feedback);
        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }
}

public record CreateSubmissionRequest(Guid AssignmentId, string Content, string? FileUrl);

public record UpdateSubmissionRequest(string Content, string? FileUrl);

public record GradeSubmissionRequest(int Marks, string Feedback);

public record ReturnForRevisionRequest(string Feedback);
