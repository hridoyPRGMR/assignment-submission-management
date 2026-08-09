using AssignmentSystem.Application.Abstractions;
using AssignmentSystem.Application.Features.Auth;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace AssignmentSystem.Application.Tests.Features.Auth;

public class ChangePasswordCommandHandlerTests
{
    private static IConfiguration CreateConfiguration() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Jwt:RefreshTokenDays"] = "7" })
            .Build();

    private static AuthService CreateService(
        AssignmentSystem.Infrastructure.Persistence.AppDbContext dbContext,
        IPasswordHasher passwordHasher) =>
        new(
            new AssignmentSystem.Infrastructure.Persistence.Repositories.UserRepository(dbContext),
            new AssignmentSystem.Infrastructure.Persistence.Repositories.RefreshTokenRepository(dbContext),
            dbContext,
            passwordHasher,
            Mock.Of<IJwtTokenGenerator>(),
            CreateConfiguration(),
            new LoginInputValidator(),
            new LogoutInputValidator(),
            new RefreshTokenInputValidator(),
            new ChangePasswordInputValidator());

    [Fact]
    public async Task ChangePassword_with_correct_current_password_should_succeed()
    {
        using var dbContext = TestDbContextFactory.Create();
        var passwordHasherMock = new Mock<IPasswordHasher>();

        var user = User.Create("teacher@school.edu", "old-hash", "Jane Smith", UserRole.Teacher, DateTime.UtcNow).Value;
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        passwordHasherMock.Setup(p => p.Verify("old-password", "old-hash")).Returns(true);
        passwordHasherMock.Setup(p => p.Hash("new-password-123")).Returns("new-hash");

        var service = CreateService(dbContext, passwordHasherMock.Object);

        var result = await service.ChangePasswordAsync(
            new ChangePasswordInput(user.Id, "old-password", "new-password-123"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var persisted = await dbContext.Users.FindAsync(user.Id);
        persisted!.PasswordHash.Should().Be("new-hash");
    }

    [Fact]
    public async Task ChangePassword_with_wrong_current_password_should_fail()
    {
        using var dbContext = TestDbContextFactory.Create();
        var passwordHasherMock = new Mock<IPasswordHasher>();

        var user = User.Create("teacher@school.edu", "old-hash", "Jane Smith", UserRole.Teacher, DateTime.UtcNow).Value;
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        passwordHasherMock.Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var service = CreateService(dbContext, passwordHasherMock.Object);

        var result = await service.ChangePasswordAsync(
            new ChangePasswordInput(user.Id, "wrong-password", "new-password-123"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Current password is incorrect.");
    }

    [Fact]
    public async Task ChangePassword_with_too_short_new_password_should_fail_validation()
    {
        using var dbContext = TestDbContextFactory.Create();
        var passwordHasherMock = new Mock<IPasswordHasher>();

        var user = User.Create("teacher@school.edu", "old-hash", "Jane Smith", UserRole.Teacher, DateTime.UtcNow).Value;
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        passwordHasherMock.Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        var service = CreateService(dbContext, passwordHasherMock.Object);

        var result = await service.ChangePasswordAsync(
            new ChangePasswordInput(user.Id, "old-password", "short"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ChangePassword_for_unknown_user_should_fail()
    {
        using var dbContext = TestDbContextFactory.Create();
        var passwordHasherMock = new Mock<IPasswordHasher>();

        var service = CreateService(dbContext, passwordHasherMock.Object);

        var result = await service.ChangePasswordAsync(
            new ChangePasswordInput(Guid.NewGuid(), "old-password", "new-password-123"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}
