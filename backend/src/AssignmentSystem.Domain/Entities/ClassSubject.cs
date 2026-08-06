namespace AssignmentSystem.Domain.Entities;

public class ClassSubject
{
    public Guid Id { get; private set; }
    public Guid ClassId { get; private set; }
    public Class Class { get; private set; } = null!;
    public Guid SubjectId { get; private set; }
    public Subject Subject { get; private set; } = null!;

    private ClassSubject() { }

    private ClassSubject(Guid id, Guid classId, Guid subjectId)
    {
        Id = id;
        ClassId = classId;
        SubjectId = subjectId;
    }

    public static ClassSubject Create(Guid classId, Guid subjectId) =>
        new(Guid.NewGuid(), classId, subjectId);
}
