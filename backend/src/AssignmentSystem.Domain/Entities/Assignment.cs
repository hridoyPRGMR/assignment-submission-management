using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Domain.Entities;

public class Assignment
{
    public Guid Id { get; private set; }
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
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

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
        Id = id;
        Title = title;
        Description = description;
        ClassId = classId;
        SubjectId = subjectId;
        TeacherUserId = teacherUserId;
        DeadlineUtc = deadlineUtc;
        MaxMarks = maxMarks;
        Status = AssignmentStatus.Draft;
        IsDeleted = false;
        CreatedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
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

    public Result Publish()
    {
        if (Status == AssignmentStatus.Published)
        {
            return Result.Failure("Assignment is already published.");
        }

        Status = AssignmentStatus.Published;
        return Result.Success();
    }

    public Result Unpublish()
    {
        if (Status == AssignmentStatus.Draft)
        {
            return Result.Failure("Assignment is already a draft.");
        }

        Status = AssignmentStatus.Draft;
        return Result.Success();
    }

    public Result UpdateDetails(
        string title,
        string description,
        DateTime deadlineUtc,
        int maxMarks,
        bool hasSubmissions,
        DateTime nowUtc)
    {
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
        UpdatedAtUtc = nowUtc;

        return Result.Success();
    }

    public void MarkDeleted(DateTime nowUtc)
    {
        IsDeleted = true;
        UpdatedAtUtc = nowUtc;
    }
}
