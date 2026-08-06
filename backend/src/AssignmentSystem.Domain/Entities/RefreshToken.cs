using AssignmentSystem.Domain.Common;

namespace AssignmentSystem.Domain.Entities;

public class RefreshToken : AuditableEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public string TokenHash { get; private set; } = null!;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }

    private RefreshToken() { }

    private RefreshToken(Guid id, Guid userId, string tokenHash, DateTime expiresAtUtc, DateTime nowUtc)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        InitializeAudit(id, nowUtc, userId);
    }

    public static RefreshToken Create(Guid userId, string tokenHash, DateTime expiresAtUtc, DateTime nowUtc) =>
        new(Guid.NewGuid(), userId, tokenHash, expiresAtUtc, nowUtc);

    public bool IsActive(DateTime nowUtc) => RevokedAtUtc is null && ExpiresAtUtc > nowUtc;

    public void Revoke(DateTime nowUtc)
    {
        RevokedAtUtc = nowUtc;
        TouchUpdated(nowUtc, UserId);
    }
}
