using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Domain.Entities;

public class Assignment : AuditableEntity
{
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public Guid ClassId { get; private set; }
    public Class Class { get; private set; } = null!;
    public Guid SubjectId { get; private set; }
    public Subject Subject { get; private set; } = null!;
    public Guid TeacherUserId { get; private set; }
    public User TeacherUser { get; private set; } = null!;
    public DateTime DeadlineUtc { get; private set; }
    public int MaxMarks { get; private set; }
    public AssignmentStatus Status { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeadlineProcessedAtUtc { get; private set; }

    private Assignment() { }

    private Assignment(
        Guid id,
        string title,
        string description,
        Guid classId,
        Guid subjectId,
        Guid teacherUserId,
        DateTime deadlineUtc,
        int maxMarks,
        DateTime nowUtc)
    {
        Title = title;
        Description = description;
        ClassId = classId;
        SubjectId = subjectId;
        TeacherUserId = teacherUserId;
        DeadlineUtc = deadlineUtc;
        MaxMarks = maxMarks;
        Status = AssignmentStatus.Draft;
        IsDeleted = false;
        InitializeAudit(id, nowUtc, teacherUserId);
    }

    public static Result<Assignment> Create(
        string title,
        string description,
        Guid classId,
        Guid subjectId,
        Guid teacherUserId,
        DateTime deadlineUtc,
        int maxMarks,
        DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure<Assignment>("Title is required.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return Result.Failure<Assignment>("Description is required.");
        }

        if (maxMarks <= 0)
        {
            return Result.Failure<Assignment>("Maximum marks must be greater than zero.");
        }

        if (deadlineUtc <= nowUtc)
        {
            return Result.Failure<Assignment>("Deadline must be in the future.");
        }

        var assignment = new Assignment(
            Guid.NewGuid(),
            title.Trim(),
            description.Trim(),
            classId,
            subjectId,
            teacherUserId,
            deadlineUtc,
            maxMarks,
            nowUtc);

        return Result.Success(assignment);
    }

    public AssignmentLifecycleStatus GetLifecycleStatus(DateTime nowUtc) => Status switch
    {
        AssignmentStatus.Draft => AssignmentLifecycleStatus.Draft,
        AssignmentStatus.Published => nowUtc <= DeadlineUtc
            ? AssignmentLifecycleStatus.SubmissionOpen
            : AssignmentLifecycleStatus.DeadlinePassed,
        AssignmentStatus.Graded => AssignmentLifecycleStatus.Graded,
        AssignmentStatus.Archived => AssignmentLifecycleStatus.Archived,
        _ => AssignmentLifecycleStatus.Draft
    };

    public Result Publish(DateTime nowUtc, Guid? actorUserId = null)
    {
        if (Status != AssignmentStatus.Draft)
        {
            return Result.Failure("Only a draft assignment can be published.");
        }

        Status = AssignmentStatus.Published;
        TouchUpdated(nowUtc, actorUserId);
        return Result.Success();
    }

    public Result Unpublish(DateTime nowUtc, Guid? actorUserId = null)
    {
        if (Status != AssignmentStatus.Published)
        {
            return Result.Failure("Only a published assignment can be unpublished.");
        }

        if (nowUtc > DeadlineUtc)
        {
            return Result.Failure("Cannot unpublish an assignment once its deadline has passed; grade or archive it instead.");
        }

        Status = AssignmentStatus.Draft;
        TouchUpdated(nowUtc, actorUserId);
        return Result.Success();
    }

    public Result MarkGraded(DateTime nowUtc, Guid? actorUserId = null)
    {
        if (Status != AssignmentStatus.Published)
        {
            return Result.Failure("Only a published assignment can be marked as graded.");
        }

        if (nowUtc <= DeadlineUtc)
        {
            return Result.Failure("Cannot mark an assignment as graded before its deadline has passed.");
        }

        Status = AssignmentStatus.Graded;
        TouchUpdated(nowUtc, actorUserId);
        return Result.Success();
    }

    public Result Archive(DateTime nowUtc, Guid? actorUserId = null)
    {
        if (Status != AssignmentStatus.Graded)
        {
            return Result.Failure("Only a graded assignment can be archived.");
        }

        Status = AssignmentStatus.Archived;
        TouchUpdated(nowUtc, actorUserId);
        return Result.Success();
    }

    public Result UpdateDetails(
        string title,
        string description,
        DateTime deadlineUtc,
        int maxMarks,
        bool hasSubmissions,
        DateTime nowUtc,
        Guid? actorUserId = null)
    {
        if (Status == AssignmentStatus.Graded || Status == AssignmentStatus.Archived)
        {
            return Result.Failure("Cannot edit an assignment that has been graded or archived.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure("Title is required.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return Result.Failure("Description is required.");
        }

        if (maxMarks <= 0)
        {
            return Result.Failure("Maximum marks must be greater than zero.");
        }

        if (hasSubmissions && (deadlineUtc != DeadlineUtc || maxMarks != MaxMarks))
        {
            return Result.Failure("Deadline and maximum marks cannot be changed once submissions exist.");
        }

        Title = title.Trim();
        Description = description.Trim();
        DeadlineUtc = deadlineUtc;
        MaxMarks = maxMarks;
        TouchUpdated(nowUtc, actorUserId);

        return Result.Success();
    }

    public void MarkDeleted(DateTime nowUtc, Guid? actorUserId = null)
    {
        IsDeleted = true;
        TouchUpdated(nowUtc, actorUserId);
    }

    public void MarkDeadlineProcessed(DateTime nowUtc)
    {
        DeadlineProcessedAtUtc = nowUtc;
    }
}
