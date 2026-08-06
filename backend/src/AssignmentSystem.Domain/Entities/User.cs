using AssignmentSystem.Domain.Common;
using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private User() { }

    private User(Guid id, string email, string passwordHash, string fullName, UserRole role, DateTime nowUtc)
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
        Role = role;
        IsActive = true;
        CreatedAtUtc = nowUtc;
    }

    public static Result<User> Create(string email, string passwordHash, string fullName, UserRole role, DateTime nowUtc)
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

        var user = new User(Guid.NewGuid(), email.Trim().ToLowerInvariant(), passwordHash, fullName.Trim(), role, nowUtc);
        return Result.Success(user);
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

    public void ChangePasswordHash(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
        {
            throw new ArgumentException("Password hash cannot be empty.", nameof(newPasswordHash));
        }

        PasswordHash = newPasswordHash;
    }

    public void UpdateProfile(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Full name cannot be empty.", nameof(fullName));
        }

        FullName = fullName.Trim();
    }
}
