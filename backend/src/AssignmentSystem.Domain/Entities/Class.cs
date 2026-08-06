using AssignmentSystem.Domain.Common;

namespace AssignmentSystem.Domain.Entities;

public class Class : AuditableEntity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }

    private Class() { }

    private Class(Guid id, string name, string? description, DateTime nowUtc, Guid? actorUserId)
    {
        Name = name;
        Description = description;
        InitializeAudit(id, nowUtc, actorUserId);
    }

    public static Result<Class> Create(string name, string? description, DateTime nowUtc, Guid? actorUserId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Class>("Class name is required.");
        }

        return Result.Success(new Class(Guid.NewGuid(), name.Trim(), description?.Trim(), nowUtc, actorUserId));
    }

    public void Update(string name, string? description, DateTime nowUtc, Guid? actorUserId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Class name cannot be empty.", nameof(name));
        }

        Name = name.Trim();
        Description = description?.Trim();
        TouchUpdated(nowUtc, actorUserId);
    }
}
