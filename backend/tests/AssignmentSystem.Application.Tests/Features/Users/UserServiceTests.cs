using AssignmentSystem.Application.Abstractions;
using AssignmentSystem.Application.Features.Users;
using AssignmentSystem.Application.Tests.TestDoubles;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using AssignmentSystem.Infrastructure.Persistence;
using AssignmentSystem.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AssignmentSystem.Application.Tests.Features.Users;

public class UserServiceTests
{
    private static readonly DateTime Now = DateTime.UtcNow;
    private static readonly Guid AdminId = Guid.NewGuid();

    private static UserService CreateService(AppDbContext dbContext, ILogger<UserService>? logger = null) =>
        new(
            new UserRepository(dbContext),
            new TeacherAssignmentRepository(dbContext),
            new StudentEnrollmentRepository(dbContext),
            new AssignmentRepository(dbContext),
            new SubmissionRepository(dbContext),
            dbContext,
            Mock.Of<IPasswordHasher>(h => h.Hash(It.IsAny<string>()) == "hashed"),
            new CreateUserInputValidator(),
            new UpdateUserInputValidator(),
            logger ?? new CapturingLogger<UserService>());

    [Fact]
    public async Task CreateAsync_on_success_should_log_the_creation()
    {
        using var dbContext = TestDbContextFactory.Create();
        var logger = new CapturingLogger<UserService>();
        var service = CreateService(dbContext, logger);

        var result = await service.CreateAsync(
            new CreateUserInput("new.teacher@school.edu", "Password1!", "Jane Smith", UserRole.Teacher), AdminId, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        logger.Entries.Should().Contain(e =>
            e.Level == LogLevel.Information &&
            e.Message.Contains(AdminId.ToString()) &&
            e.Message.Contains(result.Value.Id.ToString()));
    }

    [Fact]
    public async Task UpdateAsync_when_changing_role_should_log_the_role_change()
    {
        using var dbContext = TestDbContextFactory.Create();
        var user = User.Create("teacher@school.edu", "hashed", "Jane Smith", UserRole.Teacher, Now).Value;
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var logger = new CapturingLogger<UserService>();
        var service = CreateService(dbContext, logger);

        var result = await service.UpdateAsync(
            new UpdateUserInput(user.Id, "Jane Smith", UserRole.Admin), AdminId, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        logger.Entries.Should().Contain(e =>
            e.Level == LogLevel.Information &&
            e.Message.Contains(AdminId.ToString()) &&
            e.Message.Contains(user.Id.ToString()));
    }

    [Fact]
    public async Task UpdateAsync_when_not_changing_role_should_not_log_a_role_change()
    {
        using var dbContext = TestDbContextFactory.Create();
        var user = User.Create("teacher@school.edu", "hashed", "Jane Smith", UserRole.Teacher, Now).Value;
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var logger = new CapturingLogger<UserService>();
        var service = CreateService(dbContext, logger);

        var result = await service.UpdateAsync(
            new UpdateUserInput(user.Id, "Jane S. Smith", null), AdminId, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        logger.Entries.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_on_success_should_log_the_deletion()
    {
        using var dbContext = TestDbContextFactory.Create();
        var user = User.Create("teacher@school.edu", "hashed", "Jane Smith", UserRole.Teacher, Now).Value;
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var logger = new CapturingLogger<UserService>();
        var service = CreateService(dbContext, logger);

        var result = await service.DeleteAsync(user.Id, AdminId, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        logger.Entries.Should().Contain(e =>
            e.Level == LogLevel.Information &&
            e.Message.Contains(AdminId.ToString()) &&
            e.Message.Contains(user.Id.ToString()));
    }

    [Fact]
    public async Task SetActiveStatusAsync_on_success_should_log_the_status_change()
    {
        using var dbContext = TestDbContextFactory.Create();
        var user = User.Create("teacher@school.edu", "hashed", "Jane Smith", UserRole.Teacher, Now).Value;
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var logger = new CapturingLogger<UserService>();
        var service = CreateService(dbContext, logger);

        var result = await service.SetActiveStatusAsync(user.Id, false, AdminId, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        logger.Entries.Should().Contain(e =>
            e.Level == LogLevel.Information &&
            e.Message.Contains(AdminId.ToString()) &&
            e.Message.Contains(user.Id.ToString()));
    }
}
