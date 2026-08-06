namespace AssignmentSystem.Domain.Entities;

public class TeacherAssignment
{
    public Guid Id { get; private set; }
    public Guid TeacherUserId { get; private set; }
    public User TeacherUser { get; private set; } = null!;
    public Guid ClassId { get; private set; }
    public Class Class { get; private set; } = null!;
    public Guid SubjectId { get; private set; }
    public Subject Subject { get; private set; } = null!;

    private TeacherAssignment() { }

    private TeacherAssignment(Guid id, Guid teacherUserId, Guid classId, Guid subjectId)
    {
        Id = id;
        TeacherUserId = teacherUserId;
        ClassId = classId;
        SubjectId = subjectId;
    }

    public static TeacherAssignment Create(Guid teacherUserId, Guid classId, Guid subjectId) =>
        new(Guid.NewGuid(), teacherUserId, classId, subjectId);
}
