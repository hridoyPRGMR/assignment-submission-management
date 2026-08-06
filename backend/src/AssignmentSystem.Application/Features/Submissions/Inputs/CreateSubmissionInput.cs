namespace AssignmentSystem.Application.Features.Submissions;

public record CreateSubmissionInput(Guid StudentUserId, Guid AssignmentId, string Content, string? FileUrl);
