using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Domain.Entities;

public class Submission : AuditableEntity
{
    public Guid AssignmentId { get; private set; }
    public Assignment Assignment { get; private set; } = null!;
    public Guid StudentUserId { get; private set; }
    public User StudentUser { get; private set; } = null!;
    public string Content { get; private set; } = null!;
    public string? FileUrl { get; private set; }
    public DateTime SubmittedAtUtc { get; private set; }
    public bool IsLate { get; private set; }
    public SubmissionStatus Status { get; private set; }
    public int? Marks { get; private set; }
    public string? Feedback { get; private set; }
    public DateTime? GradedAtUtc { get; private set; }
    public Guid? GradedByUserId { get; private set; }

    private Submission() { }

    private Submission(Guid id, Guid assignmentId, Guid studentUserId, string content, string? fileUrl, DateTime nowUtc)
    {
        AssignmentId = assignmentId;
        StudentUserId = studentUserId;
        Content = content;
        FileUrl = fileUrl;
        SubmittedAtUtc = nowUtc;
        IsLate = false;
        Status = SubmissionStatus.Submitted;
        InitializeAudit(id, nowUtc, studentUserId);
    }

    public static Result<Submission> Create(
        Guid assignmentId,
        Guid studentUserId,
        string content,
        string? fileUrl,
        DateTime assignmentDeadlineUtc,
        DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return Result.Failure<Submission>("Submission content is required.");
        }

        if (nowUtc > assignmentDeadlineUtc)
        {
            return Result.Failure<Submission>("The deadline for this assignment has passed.");
        }

        return Result.Success(new Submission(Guid.NewGuid(), assignmentId, studentUserId, content.Trim(), fileUrl, nowUtc));
    }

    public Result Update(string content, string? fileUrl, DateTime assignmentDeadlineUtc, DateTime nowUtc, Guid? actorUserId = null)
    {
        if (Status != SubmissionStatus.Submitted && Status != SubmissionStatus.ReturnedForRevision)
        {
            return Result.Failure("This submission can no longer be updated.");
        }

        if (nowUtc > assignmentDeadlineUtc)
        {
            return Result.Failure("The deadline for this assignment has passed.");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return Result.Failure("Submission content is required.");
        }

        var wasReturnedForRevision = Status == SubmissionStatus.ReturnedForRevision;

        Content = content.Trim();
        FileUrl = fileUrl;
        SubmittedAtUtc = nowUtc;

        if (wasReturnedForRevision)
        {
            Status = SubmissionStatus.Submitted;
            Marks = null;
            Feedback = null;
            GradedAtUtc = null;
            GradedByUserId = null;
        }

        TouchUpdated(nowUtc, actorUserId);
        return Result.Success();
    }

    public Result StartReview(DateTime nowUtc, Guid? actorUserId = null)
    {
        if (Status != SubmissionStatus.Submitted)
        {
            return Result.Failure("Only submitted submissions can be moved to under review.");
        }

        Status = SubmissionStatus.UnderReview;
        TouchUpdated(nowUtc, actorUserId);
        return Result.Success();
    }

    public Result Grade(int marks, string feedback, int assignmentMaxMarks, Guid graderUserId, DateTime nowUtc)
    {
        if (Status != SubmissionStatus.Submitted && Status != SubmissionStatus.UnderReview)
        {
            return Result.Failure("Only submitted or under-review submissions can be graded.");
        }

        if (marks < 0 || marks > assignmentMaxMarks)
        {
            return Result.Failure($"Marks must be between 0 and {assignmentMaxMarks}.");
        }

        if (string.IsNullOrWhiteSpace(feedback))
        {
            return Result.Failure("Feedback is required when grading.");
        }

        Marks = marks;
        Feedback = feedback.Trim();
        Status = SubmissionStatus.Graded;
        GradedAtUtc = nowUtc;
        GradedByUserId = graderUserId;

        TouchUpdated(nowUtc, graderUserId);
        return Result.Success();
    }

    public Result ReturnForRevision(string feedback, DateTime nowUtc, Guid? actorUserId = null)
    {
        if (Status != SubmissionStatus.UnderReview && Status != SubmissionStatus.Graded)
        {
            return Result.Failure("Only under-review or graded submissions can be returned for revision.");
        }

        if (string.IsNullOrWhiteSpace(feedback))
        {
            return Result.Failure("Feedback is required when returning a submission for revision.");
        }

        Status = SubmissionStatus.ReturnedForRevision;
        Feedback = feedback.Trim();
        Marks = null;
        GradedAtUtc = null;
        GradedByUserId = null;

        TouchUpdated(nowUtc, actorUserId);
        return Result.Success();
    }
}
