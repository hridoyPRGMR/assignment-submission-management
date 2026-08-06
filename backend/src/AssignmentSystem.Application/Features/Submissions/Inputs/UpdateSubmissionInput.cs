namespace AssignmentSystem.Application.Features.Submissions;

public record UpdateSubmissionInput(
    Guid Id,
    Guid RequestingStudentUserId,
    string Content,
    bool ReplaceAttachment,
    string? FileName,
    string? StoredFileName,
    string? FileContentType,
    long? FileSizeBytes);
