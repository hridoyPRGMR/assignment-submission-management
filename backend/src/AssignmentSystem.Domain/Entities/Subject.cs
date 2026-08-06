using AssignmentSystem.Domain.Common;

namespace AssignmentSystem.Domain.Entities;

public class Subject : AuditableEntity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }

    private Subject() { }

    private Subject(Guid id, string name, string? description, DateTime nowUtc, Guid? actorUserId)
    {
        Name = name;
        Description = description;
        InitializeAudit(id, nowUtc, actorUserId);
    }

    public static Result<Subject> Create(string name, string? description, DateTime nowUtc, Guid? actorUserId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Subject>("Subject name is required.");
        }

        return Result.Success(new Subject(Guid.NewGuid(), name.Trim(), description?.Trim(), nowUtc, actorUserId));
    }

    public void Update(string name, string? description, DateTime nowUtc, Guid? actorUserId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Subject name cannot be empty.", nameof(name));
        }

        Name = name.Trim();
        Description = description?.Trim();
        TouchUpdated(nowUtc, actorUserId);
    }
}
