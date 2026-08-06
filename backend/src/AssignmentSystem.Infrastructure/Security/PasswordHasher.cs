using AssignmentSystem.Application.Abstractions;
using AssignmentSystem.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AssignmentSystem.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    private readonly Microsoft.AspNetCore.Identity.PasswordHasher<User> _identityHasher = new();

    public string Hash(string password) => _identityHasher.HashPassword(null!, password);

    public bool Verify(string password, string passwordHash) =>
        _identityHasher.VerifyHashedPassword(null!, passwordHash, password) != PasswordVerificationResult.Failed;
}
