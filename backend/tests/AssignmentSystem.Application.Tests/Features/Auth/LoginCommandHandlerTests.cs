using AssignmentSystem.Application.Abstractions;
using AssignmentSystem.Application.Features.Auth;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace AssignmentSystem.Application.Tests.Features.Auth;

public class LoginCommandHandlerTests
{
    private static IConfiguration CreateConfiguration() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Jwt:RefreshTokenDays"] = "7" })
            .Build();

    [Fact]
    public async Task Login_with_valid_credentials_should_succeed()
    {
        using var dbContext = TestDbContextFactory.Create();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var jwtGeneratorMock = new Mock<IJwtTokenGenerator>();

        var user = User.Create("teacher@school.edu", "hashed", "Jane Smith", UserRole.Teacher, DateTime.UtcNow).Value;
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        passwordHasherMock.Setup(p => p.Verify("correct-password", "hashed")).Returns(true);
        jwtGeneratorMock.Setup(j => j.GenerateAccessToken(It.IsAny<User>()))
            .Returns(new AccessToken("fake-jwt", DateTime.UtcNow.AddMinutes(30)));

        var handler = new LoginCommandHandler(dbContext, passwordHasherMock.Object, jwtGeneratorMock.Object, CreateConfiguration());

        var result = await handler.Handle(new LoginCommand("teacher@school.edu", "correct-password"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("fake-jwt");
        result.Value.Role.Should().Be("Teacher");
    }

    [Fact]
    public async Task Login_with_wrong_password_should_fail_with_generic_message()
    {
        using var dbContext = TestDbContextFactory.Create();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var jwtGeneratorMock = new Mock<IJwtTokenGenerator>();

        var user = User.Create("teacher@school.edu", "hashed", "Jane Smith", UserRole.Teacher, DateTime.UtcNow).Value;
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        passwordHasherMock.Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var handler = new LoginCommandHandler(dbContext, passwordHasherMock.Object, jwtGeneratorMock.Object, CreateConfiguration());

        var result = await handler.Handle(new LoginCommand("teacher@school.edu", "wrong-password"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task Login_for_deactivated_user_should_fail()
    {
        using var dbContext = TestDbContextFactory.Create();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var jwtGeneratorMock = new Mock<IJwtTokenGenerator>();

        var user = User.Create("teacher@school.edu", "hashed", "Jane Smith", UserRole.Teacher, DateTime.UtcNow).Value;
        user.Deactivate();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        passwordHasherMock.Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        var handler = new LoginCommandHandler(dbContext, passwordHasherMock.Object, jwtGeneratorMock.Object, CreateConfiguration());

        var result = await handler.Handle(new LoginCommand("teacher@school.edu", "correct-password"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Login_for_unknown_email_should_fail()
    {
        using var dbContext = TestDbContextFactory.Create();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var jwtGeneratorMock = new Mock<IJwtTokenGenerator>();

        var handler = new LoginCommandHandler(dbContext, passwordHasherMock.Object, jwtGeneratorMock.Object, CreateConfiguration());

        var result = await handler.Handle(new LoginCommand("nobody@school.edu", "whatever"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}
