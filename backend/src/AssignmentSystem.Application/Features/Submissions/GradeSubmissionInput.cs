namespace AssignmentSystem.Application.Features.Submissions;

public record GradeSubmissionInput(Guid SubmissionId, Guid RequestingTeacherUserId, int Marks, string Feedback);
