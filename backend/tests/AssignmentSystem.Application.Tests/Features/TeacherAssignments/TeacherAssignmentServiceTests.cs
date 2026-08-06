using AssignmentSystem.Application.Features.TeacherAssignments;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using AssignmentSystem.Infrastructure.Persistence;
using AssignmentSystem.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Xunit;

namespace AssignmentSystem.Application.Tests.Features.TeacherAssignments;

public class TeacherAssignmentServiceTests
{
    private static readonly DateTime Now = DateTime.UtcNow;

    private static TeacherAssignmentService CreateService(AppDbContext dbContext) =>
        new(
            new ClassSubjectRepository(dbContext),
            new ClassRepository(dbContext),
            new SubjectRepository(dbContext),
            new TeacherAssignmentRepository(dbContext),
            new UserRepository(dbContext),
            dbContext,
            new AssignTeacherInputValidator());

    private static (Class Class, Subject Subject, User Teacher) SeedClassSubjectAndTeacher(AppDbContext dbContext)
    {
        var classEntity = Class.Create("Grade 10 - A", null, Now).Value;
        var subject = Subject.Create("Mathematics", null, Now).Value;
        var teacher = User.Create("teacher@school.edu", "hashed", "Jane Smith", UserRole.Teacher, Now).Value;

        dbContext.Classes.Add(classEntity);
        dbContext.Subjects.Add(subject);
        dbContext.Users.Add(teacher);
        dbContext.ClassSubjects.Add(ClassSubject.Create(classEntity.Id, subject.Id));
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
            new AssignTeacherInput(teacher.Id, classEntity.Id, subject.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not associated");
    }

    [Fact]
    public async Task AssignAsync_when_already_assigned_should_fail()
    {
        using var dbContext = TestDbContextFactory.Create();
        var (classEntity, subject, teacher) = SeedClassSubjectAndTeacher(dbContext);

        var service = CreateService(dbContext);

        var first = await service.AssignAsync(new AssignTeacherInput(teacher.Id, classEntity.Id, subject.Id), CancellationToken.None);
        first.IsSuccess.Should().BeTrue();

        var duplicate = await service.AssignAsync(
            new AssignTeacherInput(teacher.Id, classEntity.Id, subject.Id), CancellationToken.None);

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
        dbContext.ClassSubjects.Add(ClassSubject.Create(classEntity.Id, subject.Id));
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var result = await service.AssignAsync(
            new AssignTeacherInput(student.Id, classEntity.Id, subject.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not an active teacher");
    }
}
