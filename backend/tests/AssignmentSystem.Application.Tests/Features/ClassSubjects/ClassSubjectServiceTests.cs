using AssignmentSystem.Application.Features.ClassSubjects;
using AssignmentSystem.Application.Tests.TestDoubles;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Infrastructure.Persistence;
using AssignmentSystem.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AssignmentSystem.Application.Tests.Features.ClassSubjects;

public class ClassSubjectServiceTests
{
    private static readonly DateTime Now = DateTime.UtcNow;
    private static readonly Guid AdminId = Guid.NewGuid();

    private static ClassSubjectService CreateService(AppDbContext dbContext, ILogger<ClassSubjectService>? logger = null) =>
        new(
            new ClassSubjectRepository(dbContext),
            new ClassRepository(dbContext),
            new SubjectRepository(dbContext),
            new TeacherAssignmentRepository(dbContext),
            dbContext,
            new AssignSubjectToClassInputValidator(),
            logger ?? new CapturingLogger<ClassSubjectService>());

    [Fact]
    public async Task AssignAsync_on_success_should_log_the_association()
    {
        using var dbContext = TestDbContextFactory.Create();

        var classEntity = Class.Create("Grade 10 - A", null, Now).Value;
        var subject = Subject.Create("Mathematics", null, Now).Value;

        dbContext.Classes.Add(classEntity);
        dbContext.Subjects.Add(subject);
        await dbContext.SaveChangesAsync();

        var logger = new CapturingLogger<ClassSubjectService>();
        var service = CreateService(dbContext, logger);

        var result = await service.AssignAsync(
            new AssignSubjectToClassInput(classEntity.Id, subject.Id), AdminId, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        logger.Entries.Should().Contain(e =>
            e.Level == LogLevel.Information &&
            e.Message.Contains(AdminId.ToString()) &&
            e.Message.Contains(subject.Id.ToString()));
    }

    [Fact]
    public async Task RemoveAsync_on_success_should_log_the_removal()
    {
        using var dbContext = TestDbContextFactory.Create();

        var classEntity = Class.Create("Grade 10 - A", null, Now).Value;
        var subject = Subject.Create("Mathematics", null, Now).Value;

        dbContext.Classes.Add(classEntity);
        dbContext.Subjects.Add(subject);
        await dbContext.SaveChangesAsync();

        var logger = new CapturingLogger<ClassSubjectService>();
        var service = CreateService(dbContext, logger);

        var assigned = await service.AssignAsync(
            new AssignSubjectToClassInput(classEntity.Id, subject.Id), AdminId, CancellationToken.None);

        var result = await service.RemoveAsync(assigned.Value.Id, AdminId, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        logger.Entries.Should().Contain(e =>
            e.Level == LogLevel.Information &&
            e.Message.Contains(AdminId.ToString()) &&
            e.Message.Contains(assigned.Value.Id.ToString()));
    }

    [Fact]
    public async Task RemoveAsync_when_teachers_assigned_should_fail()
    {
        using var dbContext = TestDbContextFactory.Create();

        var classEntity = Class.Create("Grade 10 - A", null, Now).Value;
        var subject = Subject.Create("Mathematics", null, Now).Value;
        var teacher = User.Create("teacher@school.edu", "hashed", "Jane Smith", Domain.Enums.UserRole.Teacher, Now).Value;
        var classSubject = ClassSubject.Create(classEntity.Id, subject.Id, Now);
        var teacherAssignment = TeacherAssignment.Create(teacher.Id, classEntity.Id, subject.Id, Now);

        dbContext.Classes.Add(classEntity);
        dbContext.Subjects.Add(subject);
        dbContext.Users.Add(teacher);
        dbContext.ClassSubjects.Add(classSubject);
        dbContext.TeacherAssignments.Add(teacherAssignment);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var result = await service.RemoveAsync(classSubject.Id, AdminId, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Cannot remove");
    }
}
