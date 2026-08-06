using AssignmentSystem.Domain.Common;

namespace AssignmentSystem.Domain.Entities;

public class TeacherAssignment : AuditableEntity
{
    public Guid TeacherUserId { get; private set; }
    public User TeacherUser { get; private set; } = null!;
    public Guid ClassId { get; private set; }
    public Class Class { get; private set; } = null!;
    public Guid SubjectId { get; private set; }
    public Subject Subject { get; private set; } = null!;

    private TeacherAssignment() { }

    private TeacherAssignment(Guid id, Guid teacherUserId, Guid classId, Guid subjectId, DateTime nowUtc, Guid? actorUserId)
    {
        TeacherUserId = teacherUserId;
        ClassId = classId;
        SubjectId = subjectId;
        InitializeAudit(id, nowUtc, actorUserId);
    }

    public static TeacherAssignment Create(
        Guid teacherUserId, Guid classId, Guid subjectId, DateTime nowUtc, Guid? actorUserId = null) =>
        new(Guid.NewGuid(), teacherUserId, classId, subjectId, nowUtc, actorUserId);
}
