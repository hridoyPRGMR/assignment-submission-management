namespace AssignmentSystem.Domain.Entities;

public class StudentEnrollment
{
    public Guid Id { get; private set; }
    public Guid StudentUserId { get; private set; }
    public User StudentUser { get; private set; } = null!;
    public Guid ClassId { get; private set; }
    public Class Class { get; private set; } = null!;

    private StudentEnrollment() { }

    private StudentEnrollment(Guid id, Guid studentUserId, Guid classId)
    {
        Id = id;
        StudentUserId = studentUserId;
        ClassId = classId;
    }

    public static StudentEnrollment Create(Guid studentUserId, Guid classId) =>
        new(Guid.NewGuid(), studentUserId, classId);
}
