namespace AssignmentSystem.Application.Features.Submissions;

public record UpdateSubmissionInput(Guid Id, Guid RequestingStudentUserId, string Content, string? FileUrl);
