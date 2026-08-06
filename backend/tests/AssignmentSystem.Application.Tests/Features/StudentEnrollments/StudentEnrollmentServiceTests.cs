using AssignmentSystem.Application.Features.StudentEnrollments;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using AssignmentSystem.Infrastructure.Persistence;
using AssignmentSystem.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Xunit;

namespace AssignmentSystem.Application.Tests.Features.StudentEnrollments;

public class StudentEnrollmentServiceTests
{
    private static readonly DateTime Now = DateTime.UtcNow;

    private static StudentEnrollmentService CreateService(AppDbContext dbContext) =>
        new(
            new ClassRepository(dbContext),
            new StudentEnrollmentRepository(dbContext),
            new SubmissionRepository(dbContext),
            new UserRepository(dbContext),
            dbContext,
            new EnrollStudentInputValidator());

    [Fact]
    public async Task EnrollAsync_when_already_enrolled_should_fail()
    {
        using var dbContext = TestDbContextFactory.Create();

        var classEntity = Class.Create("Grade 10 - A", null, Now).Value;
        var student = User.Create("student@school.edu", "hashed", "John Doe", UserRole.Student, Now).Value;

        dbContext.Classes.Add(classEntity);
        dbContext.Users.Add(student);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var first = await service.EnrollAsync(new EnrollStudentInput(student.Id, classEntity.Id), CancellationToken.None);
        first.IsSuccess.Should().BeTrue();

        var duplicate = await service.EnrollAsync(new EnrollStudentInput(student.Id, classEntity.Id), CancellationToken.None);

        duplicate.IsFailure.Should().BeTrue();
        duplicate.Error.Should().Contain("already enrolled");
    }

    [Fact]
    public async Task EnrollAsync_for_a_non_student_user_should_fail()
    {
        using var dbContext = TestDbContextFactory.Create();

        var classEntity = Class.Create("Grade 10 - A", null, Now).Value;
        var teacher = User.Create("teacher@school.edu", "hashed", "Jane Smith", UserRole.Teacher, Now).Value;

        dbContext.Classes.Add(classEntity);
        dbContext.Users.Add(teacher);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var result = await service.EnrollAsync(new EnrollStudentInput(teacher.Id, classEntity.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not an active student");
    }

    [Fact]
    public async Task UnenrollAsync_when_student_has_submissions_in_the_class_should_fail()
    {
        using var dbContext = TestDbContextFactory.Create();

        var classEntity = Class.Create("Grade 10 - A", null, Now).Value;
        var subject = Subject.Create("Mathematics", null, Now).Value;
        var teacher = User.Create("teacher@school.edu", "hashed", "Jane Smith", UserRole.Teacher, Now).Value;
        var student = User.Create("student@school.edu", "hashed", "John Doe", UserRole.Student, Now).Value;

        var assignment = Assignment.Create(
            "Homework", "Description", classEntity.Id, subject.Id, teacher.Id, Now.AddDays(7), 100, Now).Value;
        assignment.Publish();

        var submission = Submission.Create(assignment.Id, student.Id, "My answer", null, assignment.DeadlineUtc, Now).Value;
        var enrollment = StudentEnrollment.Create(student.Id, classEntity.Id);

        dbContext.Classes.Add(classEntity);
        dbContext.Subjects.Add(subject);
        dbContext.Users.AddRange(teacher, student);
        dbContext.Assignments.Add(assignment);
        dbContext.Submissions.Add(submission);
        dbContext.StudentEnrollments.Add(enrollment);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var result = await service.UnenrollAsync(enrollment.Id, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Cannot unenroll");
    }

    [Fact]
    public async Task UnenrollAsync_when_student_has_no_submissions_should_succeed()
    {
        using var dbContext = TestDbContextFactory.Create();

        var classEntity = Class.Create("Grade 10 - A", null, Now).Value;
        var student = User.Create("student@school.edu", "hashed", "John Doe", UserRole.Student, Now).Value;
        var enrollment = StudentEnrollment.Create(student.Id, classEntity.Id);

        dbContext.Classes.Add(classEntity);
        dbContext.Users.Add(student);
        dbContext.StudentEnrollments.Add(enrollment);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var result = await service.UnenrollAsync(enrollment.Id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}
