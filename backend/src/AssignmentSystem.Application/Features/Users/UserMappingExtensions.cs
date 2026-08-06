using AssignmentSystem.Domain.Entities;

namespace AssignmentSystem.Application.Features.Users;

public static class UserMappingExtensions
{
    public static UserDto ToDto(this User user) =>
        new(user.Id, user.Email, user.FullName, user.Role.ToString(), user.IsActive, user.CreatedAtUtc);
}
