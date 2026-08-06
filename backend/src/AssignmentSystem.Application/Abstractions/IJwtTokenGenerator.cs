using AssignmentSystem.Domain.Entities;

namespace AssignmentSystem.Application.Abstractions;

public record AccessToken(string Token, DateTime ExpiresAtUtc);

public interface IJwtTokenGenerator
{
    AccessToken GenerateAccessToken(User user);
}
