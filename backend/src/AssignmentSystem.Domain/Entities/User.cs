using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Domain.Entities;

public class User : AuditableEntity
{
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }

    private User() { }

    private User(Guid id, string email, string passwordHash, string fullName, UserRole role, DateTime nowUtc, Guid? actorUserId)
    {
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
        Role = role;
        IsActive = true;
        InitializeAudit(id, nowUtc, actorUserId);
    }

    public static Result<User> Create(
        string email, string passwordHash, string fullName, UserRole role, DateTime nowUtc, Guid? actorUserId = null)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<User>("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return Result.Failure<User>("Password hash is required.");
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            return Result.Failure<User>("Full name is required.");
        }

        var user = new User(
            Guid.NewGuid(), email.Trim().ToLowerInvariant(), passwordHash, fullName.Trim(), role, nowUtc, actorUserId);
        return Result.Success(user);
    }

    public void Deactivate(DateTime nowUtc, Guid? actorUserId = null)
    {
        IsActive = false;
        TouchUpdated(nowUtc, actorUserId);
    }

    public void Activate(DateTime nowUtc, Guid? actorUserId = null)
    {
        IsActive = true;
        TouchUpdated(nowUtc, actorUserId);
    }

    public void ChangePasswordHash(string newPasswordHash, DateTime nowUtc, Guid? actorUserId = null)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
        {
            throw new ArgumentException("Password hash cannot be empty.", nameof(newPasswordHash));
        }

        PasswordHash = newPasswordHash;
        TouchUpdated(nowUtc, actorUserId);
    }

    public void UpdateProfile(string fullName, DateTime nowUtc, Guid? actorUserId = null)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Full name cannot be empty.", nameof(fullName));
        }

        FullName = fullName.Trim();
        TouchUpdated(nowUtc, actorUserId);
    }
}
