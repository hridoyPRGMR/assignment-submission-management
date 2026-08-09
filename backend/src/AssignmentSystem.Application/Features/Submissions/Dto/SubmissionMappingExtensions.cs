namespace AssignmentSystem.Application.Features.Submissions;

public static class SubmissionMappingExtensions
{
    public static SubmissionDto ToDto(this Domain.Entities.Submission submission) => new(
        submission.Id,
        submission.AssignmentId,
        submission.Assignment.Title,
        submission.Assignment.MaxMarks,
        submission.StudentUserId,
        submission.StudentUser.FullName,
        submission.Content,
        submission.FileName,
        submission.StoredFileName,
        submission.FileContentType,
        submission.FileSizeBytes,
        submission.StoredFileName is null ? null : $"/submissions/{submission.Id}/file",
        submission.SubmittedAtUtc,
        submission.IsLate,
        submission.Status.ToString(),
        submission.Marks,
        submission.Feedback,
        submission.GradedAtUtc,
        submission.GradedByUserId);
}
