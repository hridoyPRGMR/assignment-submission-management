namespace AssignmentSystem.Application.Features.Submissions;

public record SubmissionDto(
    Guid Id,
    Guid AssignmentId,
    string AssignmentTitle,
    int AssignmentMaxMarks,
    Guid StudentUserId,
    string StudentFullName,
    string Content,
    string? FileUrl,
    DateTime SubmittedAtUtc,
    bool IsLate,
    string Status,
    int? Marks,
    string? Feedback,
    DateTime? GradedAtUtc,
    Guid? GradedByUserId);
