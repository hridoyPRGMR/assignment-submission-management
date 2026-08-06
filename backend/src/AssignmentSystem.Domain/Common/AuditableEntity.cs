namespace AssignmentSystem.Domain.Common;

public abstract class AuditableEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAtUtc { get; protected set; }
    public Guid? CreatedByUserId { get; protected set; }
    public DateTime UpdatedAtUtc { get; protected set; }
    public Guid? UpdatedByUserId { get; protected set; }

    protected void InitializeAudit(Guid id, DateTime nowUtc, Guid? actorUserId)
    {
        Id = id;
        CreatedAtUtc = nowUtc;
        CreatedByUserId = actorUserId;
        UpdatedAtUtc = nowUtc;
        UpdatedByUserId = actorUserId;
    }

    protected void TouchUpdated(DateTime nowUtc, Guid? actorUserId)
    {
        UpdatedAtUtc = nowUtc;
        UpdatedByUserId = actorUserId;
    }
}
