using System.Security.Claims;
using AssignmentSystem.Api.Common;
using AssignmentSystem.Application.Abstractions;
using AssignmentSystem.Application.Common;
using AssignmentSystem.Application.Features.Submissions;
using AssignmentSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[Authorize]
public class SubmissionsController(ISubmissionService submissionService, IFileStorageService fileStorageService) : ApiControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.StudentOnly)]
    public async Task<IActionResult> GetMySubmissions(CancellationToken cancellationToken)
    {
        var result = await submissionService.GetMySubmissionsAsync(CurrentUserId, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("for-assignment/{assignmentId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.StudentOnly)]
    public async Task<IActionResult> GetMySubmissionForAssignment(Guid assignmentId, CancellationToken cancellationToken)
    {
        var result = await submissionService.GetForAssignmentAsync(assignmentId, CurrentUserId, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.StudentOnly)]
    [RequestSizeLimit(SubmissionFileConstraints.MaxFileSizeBytes + 1024)]
    public async Task<IActionResult> CreateSubmission(
        [FromForm] Guid assignmentId, [FromForm] string content, IFormFile? file, CancellationToken cancellationToken)
    {
        var fileValidationError = ValidateFile(file);
        if (fileValidationError is not null)
        {
            return BadRequest(fileValidationError);
        }

        string? storedFileName = null;
        if (file is not null)
        {
            await using var stream = file.OpenReadStream();
            storedFileName = await fileStorageService.SaveAsync(stream, file.FileName, cancellationToken);
        }

        var input = new CreateSubmissionInput(
            CurrentUserId, assignmentId, content,
            file?.FileName, storedFileName, file?.ContentType, file?.Length);

        var result = await submissionService.CreateAsync(input, cancellationToken);

        if (result.IsFailure && storedFileName is not null)
        {
            fileStorageService.Delete(storedFileName);
        }

        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.StudentOnly)]
    [RequestSizeLimit(SubmissionFileConstraints.MaxFileSizeBytes + 1024)]
    public async Task<IActionResult> UpdateSubmission(
        Guid id, [FromForm] string content, IFormFile? file, CancellationToken cancellationToken)
    {
        var fileValidationError = ValidateFile(file);
        if (fileValidationError is not null)
        {
            return BadRequest(fileValidationError);
        }

        string? storedFileName = null;
        if (file is not null)
        {
            await using var stream = file.OpenReadStream();
            storedFileName = await fileStorageService.SaveAsync(stream, file.FileName, cancellationToken);
        }

        var input = new UpdateSubmissionInput(
            id, CurrentUserId, content, ReplaceAttachment: file is not null,
            file?.FileName, storedFileName, file?.ContentType, file?.Length);

        var result = await submissionService.UpdateAsync(input, cancellationToken);

        if (result.IsFailure && storedFileName is not null)
        {
            fileStorageService.Delete(storedFileName);
        }

        return HandleResult(result);
    }

    [HttpGet("{id:guid}/file")]
    public async Task<IActionResult> DownloadFile(Guid id, CancellationToken cancellationToken)
    {
        var result = await submissionService.GetByIdAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            return NotFound();
        }

        var submission = result.Value;

        var isOwner = User.IsInRole(UserRole.Student.ToString()) && submission.StudentUserId == CurrentUserId;
        var isAdmin = User.IsInRole(UserRole.Admin.ToString());
        var isOwningTeacher = User.IsInRole(UserRole.Teacher.ToString())
            && (await submissionService.GetListForAssignmentAsync(submission.AssignmentId, CurrentUserId, isAdmin, cancellationToken)).IsSuccess;

        if (!isOwner && !isAdmin && !isOwningTeacher)
        {
            return Forbid();
        }

        if (submission.StoredFileName is null || submission.FileName is null)
        {
            return NotFound();
        }

        if (!fileStorageService.Exists(submission.StoredFileName))
        {
            return NotFound();
        }

        var stream = fileStorageService.OpenRead(submission.StoredFileName);
        return File(stream, submission.FileContentType ?? "application/octet-stream", submission.FileName);
    }

    [HttpPost("{id:guid}/start-review")]
    [Authorize(Policy = AuthorizationPolicies.TeacherOnly)]
    public async Task<IActionResult> StartReview(Guid id, CancellationToken cancellationToken)
    {
        var result = await submissionService.StartReviewAsync(id, CurrentUserId, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/grade")]
    [Authorize(Policy = AuthorizationPolicies.TeacherOnly)]
    public async Task<IActionResult> Grade(Guid id, GradeSubmissionRequest request, CancellationToken cancellationToken)
    {
        var input = new GradeSubmissionInput(id, CurrentUserId, request.Marks, request.Feedback);
        var result = await submissionService.GradeAsync(input, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/return-for-revision")]
    [Authorize(Policy = AuthorizationPolicies.TeacherOnly)]
    public async Task<IActionResult> ReturnForRevision(Guid id, ReturnForRevisionRequest request, CancellationToken cancellationToken)
    {
        var input = new ReturnSubmissionForRevisionInput(id, CurrentUserId, request.Feedback);
        var result = await submissionService.ReturnForRevisionAsync(input, cancellationToken);
        return HandleResult(result);
    }

    private static string? ValidateFile(IFormFile? file)
    {
        if (file is null)
        {
            return null;
        }

        if (file.Length > SubmissionFileConstraints.MaxFileSizeBytes)
        {
            return $"File must be smaller than {SubmissionFileConstraints.MaxFileSizeBytes / (1024 * 1024)} MB.";
        }

        var extension = Path.GetExtension(file.FileName);
        if (!SubmissionFileConstraints.AllowedExtensions.Contains(extension))
        {
            return $"File type not allowed. Allowed types: {string.Join(", ", SubmissionFileConstraints.AllowedExtensions)}.";
        }

        return null;
    }
}

public record GradeSubmissionRequest(int Marks, string Feedback);

public record ReturnForRevisionRequest(string Feedback);
