using AssignmentSystem.Domain.Common;

namespace AssignmentSystem.Domain.Entities;

public class StudentEnrollment : AuditableEntity
{
    public Guid StudentUserId { get; private set; }
    public User StudentUser { get; private set; } = null!;
    public Guid ClassId { get; private set; }
    public Class Class { get; private set; } = null!;

    private StudentEnrollment() { }

    private StudentEnrollment(Guid id, Guid studentUserId, Guid classId, DateTime nowUtc, Guid? actorUserId)
    {
        StudentUserId = studentUserId;
        ClassId = classId;
        InitializeAudit(id, nowUtc, actorUserId);
    }

    public static StudentEnrollment Create(Guid studentUserId, Guid classId, DateTime nowUtc, Guid? actorUserId = null) =>
        new(Guid.NewGuid(), studentUserId, classId, nowUtc, actorUserId);
}
