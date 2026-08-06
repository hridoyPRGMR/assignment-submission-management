using AssignmentSystem.Domain.Common;

namespace AssignmentSystem.Domain.Entities;

public class ClassSubject : AuditableEntity
{
    public Guid ClassId { get; private set; }
    public Class Class { get; private set; } = null!;
    public Guid SubjectId { get; private set; }
    public Subject Subject { get; private set; } = null!;

    private ClassSubject() { }

    private ClassSubject(Guid id, Guid classId, Guid subjectId, DateTime nowUtc, Guid? actorUserId)
    {
        ClassId = classId;
        SubjectId = subjectId;
        InitializeAudit(id, nowUtc, actorUserId);
    }

    public static ClassSubject Create(Guid classId, Guid subjectId, DateTime nowUtc, Guid? actorUserId = null) =>
        new(Guid.NewGuid(), classId, subjectId, nowUtc, actorUserId);
}
