using AssignmentSystem.Application.Features.Assignments;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using AssignmentSystem.Infrastructure.Persistence;
using AssignmentSystem.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Xunit;

namespace AssignmentSystem.Application.Tests.Features.Assignments;

public class AssignmentServiceAuthorizationTests
{
    private static readonly DateTime Now = DateTime.UtcNow;

    private static AssignmentService CreateService(AppDbContext dbContext) =>
        new(
            new AssignmentRepository(dbContext),
            new TeacherAssignmentRepository(dbContext),
            new SubmissionRepository(dbContext),
            new StudentEnrollmentRepository(dbContext),
            dbContext,
            new CreateAssignmentInputValidator(),
            new UpdateAssignmentInputValidator());

    private static (Assignment Assignment, User OwningTeacher, User OtherTeacher) SeedAssignment(AppDbContext dbContext)
    {
        var classEntity = Class.Create("Grade 10 - A", null, Now).Value;
        var subjectEntity = Subject.Create("Mathematics", null, Now).Value;
        var owningTeacher = User.Create("teacher@school.edu", "hashed", "Jane Smith", UserRole.Teacher, Now).Value;
        var otherTeacher = User.Create("other.teacher@school.edu", "hashed", "Bob Jones", UserRole.Teacher, Now).Value;

        var assignment = Assignment.Create(
            "Homework", "Description", classEntity.Id, subjectEntity.Id, owningTeacher.Id, Now.AddDays(7), 100, Now).Value;

        dbContext.Classes.Add(classEntity);
        dbContext.Subjects.Add(subjectEntity);
        dbContext.Users.AddRange(owningTeacher, otherTeacher);
        dbContext.Assignments.Add(assignment);
        dbContext.SaveChanges();

        return (assignment, owningTeacher, otherTeacher);
    }

    [Fact]
    public async Task UpdateAsync_by_non_owning_teacher_should_be_rejected()
    {
        using var dbContext = TestDbContextFactory.Create();
        var (assignment, _, otherTeacher) = SeedAssignment(dbContext);

        var service = CreateService(dbContext);

        var result = await service.UpdateAsync(
            new UpdateAssignmentInput(assignment.Id, otherTeacher.Id, "New Title", "New Description", Now.AddDays(10), 50),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("do not own");
    }

    [Fact]
    public async Task DeleteAsync_by_non_owning_teacher_should_be_rejected()
    {
        using var dbContext = TestDbContextFactory.Create();
        var (assignment, _, otherTeacher) = SeedAssignment(dbContext);

        var service = CreateService(dbContext);

        var result = await service.DeleteAsync(assignment.Id, otherTeacher.Id, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("do not own");
    }

    [Fact]
    public async Task PublishAsync_by_non_owning_teacher_should_be_rejected()
    {
        using var dbContext = TestDbContextFactory.Create();
        var (assignment, _, otherTeacher) = SeedAssignment(dbContext);

        var service = CreateService(dbContext);

        var result = await service.PublishAsync(assignment.Id, otherTeacher.Id, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("do not own");
    }

    [Fact]
    public async Task PublishAsync_by_owning_teacher_should_succeed()
    {
        using var dbContext = TestDbContextFactory.Create();
        var (assignment, owningTeacher, _) = SeedAssignment(dbContext);

        var service = CreateService(dbContext);

        var result = await service.PublishAsync(assignment.Id, owningTeacher.Id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Published");
    }

    [Fact]
    public async Task GetPublishedForStudentAsync_should_only_return_published_assignments_for_enrolled_classes()
    {
        using var dbContext = TestDbContextFactory.Create();

        var enrolledClass = Class.Create("Grade 10 - A", null, Now).Value;
        var otherClass = Class.Create("Grade 10 - B", null, Now).Value;
        var subject = Subject.Create("Mathematics", null, Now).Value;
        var teacher = User.Create("teacher@school.edu", "hashed", "Jane Smith", UserRole.Teacher, Now).Value;
        var student = User.Create("student@school.edu", "hashed", "John Doe", UserRole.Student, Now).Value;

        var publishedInEnrolledClass = Assignment.Create(
            "Published in enrolled class", "d", enrolledClass.Id, subject.Id, teacher.Id, Now.AddDays(7), 100, Now).Value;
        publishedInEnrolledClass.Publish();

        var draftInEnrolledClass = Assignment.Create(
            "Draft in enrolled class", "d", enrolledClass.Id, subject.Id, teacher.Id, Now.AddDays(7), 100, Now).Value;

        var publishedInOtherClass = Assignment.Create(
            "Published in other class", "d", otherClass.Id, subject.Id, teacher.Id, Now.AddDays(7), 100, Now).Value;
        publishedInOtherClass.Publish();

        dbContext.Classes.AddRange(enrolledClass, otherClass);
        dbContext.Subjects.Add(subject);
        dbContext.Users.AddRange(teacher, student);
        dbContext.Assignments.AddRange(publishedInEnrolledClass, draftInEnrolledClass, publishedInOtherClass);
        dbContext.StudentEnrollments.Add(StudentEnrollment.Create(student.Id, enrolledClass.Id));
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var result = await service.GetPublishedForStudentAsync(student.Id, 1, 20, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle();
        result.Value.Items.Single().Title.Should().Be("Published in enrolled class");
    }
}
