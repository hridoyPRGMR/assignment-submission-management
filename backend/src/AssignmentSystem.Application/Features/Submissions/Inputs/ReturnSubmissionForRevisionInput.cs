namespace AssignmentSystem.Application.Features.Submissions;

public record ReturnSubmissionForRevisionInput(Guid SubmissionId, Guid RequestingTeacherUserId, string Feedback);
