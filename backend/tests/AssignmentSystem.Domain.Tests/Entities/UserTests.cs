using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace AssignmentSystem.Domain.Tests.Entities;

public class UserTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_data_should_succeed()
    {
        var result = User.Create("Teacher@School.edu", "hashed-password", "Jane Smith", UserRole.Teacher, Now);

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("teacher@school.edu");
        result.Value.FullName.Should().Be("Jane Smith");
        result.Value.Role.Should().Be(UserRole.Teacher);
        result.Value.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "hash", "Name")]
    [InlineData("a@b.com", "", "Name")]
    [InlineData("a@b.com", "hash", "")]
    public void Create_with_missing_fields_should_fail(string email, string passwordHash, string fullName)
    {
        var result = User.Create(email, passwordHash, fullName, UserRole.Student, Now);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_then_activate_should_toggle_IsActive()
    {
        var user = User.Create("student@school.edu", "hash", "John Doe", UserRole.Student, Now).Value;

        user.Deactivate(Now);
        user.IsActive.Should().BeFalse();

        user.Activate(Now);
        user.IsActive.Should().BeTrue();
    }
}
