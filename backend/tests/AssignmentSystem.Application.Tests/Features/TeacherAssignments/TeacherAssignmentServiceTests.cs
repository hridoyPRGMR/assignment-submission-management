using AssignmentSystem.Application.Features.TeacherAssignments;
using AssignmentSystem.Application.Tests.TestDoubles;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using AssignmentSystem.Infrastructure.Persistence;
using AssignmentSystem.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AssignmentSystem.Application.Tests.Features.TeacherAssignments;

public class TeacherAssignmentServiceTests
{
    private static readonly DateTime Now = DateTime.UtcNow;
    private static readonly Guid AdminId = Guid.NewGuid();

    private static TeacherAssignmentService CreateService(AppDbContext dbContext, ILogger<TeacherAssignmentService>? logger = null) =>
        new(
            new ClassSubjectRepository(dbContext),
            new ClassRepository(dbContext),
            new SubjectRepository(dbContext),
            new TeacherAssignmentRepository(dbContext),
            new UserRepository(dbContext),
            dbContext,
            new AssignTeacherInputValidator(),
            logger ?? new CapturingLogger<TeacherAssignmentService>());

    private static (Class Class, Subject Subject, User Teacher) SeedClassSubjectAndTeacher(AppDbContext dbContext)
    {
        var classEntity = Class.Create("Grade 10 - A", null, Now).Value;
        var subject = Subject.Create("Mathematics", null, Now).Value;
        var teacher = User.Create("teacher@school.edu", "hashed", "Jane Smith", UserRole.Teacher, Now).Value;

        dbContext.Classes.Add(classEntity);
        dbContext.Subjects.Add(subject);
        dbContext.Users.Add(teacher);
        dbContext.ClassSubjects.Add(ClassSubject.Create(classEntity.Id, subject.Id, Now));
        dbContext.SaveChanges();

        return (classEntity, subject, teacher);
    }

    [Fact]
    public async Task AssignAsync_when_subject_not_associated_with_class_should_fail()
    {
        using var dbContext = TestDbContextFactory.Create();

        var classEntity = Class.Create("Grade 10 - A", null, Now).Value;
        var subject = Subject.Create("Mathematics", null, Now).Value;
        var teacher = User.Create("teacher@school.edu", "hashed", "Jane Smith", UserRole.Teacher, Now).Value;

        dbContext.Classes.Add(classEntity);
        dbContext.Subjects.Add(subject);
        dbContext.Users.Add(teacher);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var result = await service.AssignAsync(
            new AssignTeacherInput(teacher.Id, classEntity.Id, subject.Id), AdminId, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not associated");
    }

    [Fact]
    public async Task AssignAsync_when_already_assigned_should_fail()
    {
        using var dbContext = TestDbContextFactory.Create();
        var (classEntity, subject, teacher) = SeedClassSubjectAndTeacher(dbContext);

        var service = CreateService(dbContext);

        var first = await service.AssignAsync(
            new AssignTeacherInput(teacher.Id, classEntity.Id, subject.Id), AdminId, CancellationToken.None);
        first.IsSuccess.Should().BeTrue();

        var duplicate = await service.AssignAsync(
            new AssignTeacherInput(teacher.Id, classEntity.Id, subject.Id), AdminId, CancellationToken.None);

        duplicate.IsFailure.Should().BeTrue();
        duplicate.Error.Should().Contain("already assigned");
    }

    [Fact]
    public async Task AssignAsync_for_a_non_teacher_user_should_fail()
    {
        using var dbContext = TestDbContextFactory.Create();

        var classEntity = Class.Create("Grade 10 - A", null, Now).Value;
        var subject = Subject.Create("Mathematics", null, Now).Value;
        var student = User.Create("student@school.edu", "hashed", "John Doe", UserRole.Student, Now).Value;

        dbContext.Classes.Add(classEntity);
        dbContext.Subjects.Add(subject);
        dbContext.Users.Add(student);
        dbContext.ClassSubjects.Add(ClassSubject.Create(classEntity.Id, subject.Id, Now));
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var result = await service.AssignAsync(
            new AssignTeacherInput(student.Id, classEntity.Id, subject.Id), AdminId, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not an active teacher");
    }

    [Fact]
    public async Task AssignAsync_on_success_should_log_the_assignment()
    {
        using var dbContext = TestDbContextFactory.Create();
        var (classEntity, subject, teacher) = SeedClassSubjectAndTeacher(dbContext);
        var logger = new CapturingLogger<TeacherAssignmentService>();
        var service = CreateService(dbContext, logger);

        var result = await service.AssignAsync(
            new AssignTeacherInput(teacher.Id, classEntity.Id, subject.Id), AdminId, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        logger.Entries.Should().Contain(e =>
            e.Level == LogLevel.Information &&
            e.Message.Contains(AdminId.ToString()) &&
            e.Message.Contains(teacher.Id.ToString()));
    }

    [Fact]
    public async Task RemoveAsync_on_success_should_log_the_removal()
    {
        using var dbContext = TestDbContextFactory.Create();
        var (classEntity, subject, teacher) = SeedClassSubjectAndTeacher(dbContext);
        var logger = new CapturingLogger<TeacherAssignmentService>();
        var service = CreateService(dbContext, logger);

        var assigned = await service.AssignAsync(
            new AssignTeacherInput(teacher.Id, classEntity.Id, subject.Id), AdminId, CancellationToken.None);

        var result = await service.RemoveAsync(assigned.Value.Id, AdminId, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        logger.Entries.Should().Contain(e =>
            e.Level == LogLevel.Information &&
            e.Message.Contains(AdminId.ToString()) &&
            e.Message.Contains(assigned.Value.Id.ToString()));
    }
}
