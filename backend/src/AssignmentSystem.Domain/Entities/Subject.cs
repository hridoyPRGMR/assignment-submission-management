using AssignmentSystem.Domain.Common;

namespace AssignmentSystem.Domain.Entities;

public class Subject
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private Subject() { }

    private Subject(Guid id, string name, string? description, DateTime nowUtc)
    {
        Id = id;
        Name = name;
        Description = description;
        CreatedAtUtc = nowUtc;
    }

    public static Result<Subject> Create(string name, string? description, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Subject>("Subject name is required.");
        }

        return Result.Success(new Subject(Guid.NewGuid(), name.Trim(), description?.Trim(), nowUtc));
    }

    public void Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Subject name cannot be empty.", nameof(name));
        }

        Name = name.Trim();
        Description = description?.Trim();
    }
}
