using AssignmentSystem.Application.Features.Submissions;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace AssignmentSystem.Application.Tests.Features.Submissions;

public class CreateSubmissionCommandHandlerTests
{
    private static readonly DateTime Now = DateTime.UtcNow;

    private static SubmissionService CreateService(AssignmentSystem.Infrastructure.Persistence.AppDbContext dbContext) =>
        new(
            new AssignmentSystem.Infrastructure.Persistence.Repositories.SubmissionRepository(dbContext),
            new AssignmentSystem.Infrastructure.Persistence.Repositories.AssignmentRepository(dbContext),
            new AssignmentSystem.Infrastructure.Persistence.Repositories.StudentEnrollmentRepository(dbContext),
            dbContext,
            new CreateSubmissionInputValidator(),
            new GradeSubmissionInputValidator(),
            new ReturnSubmissionForRevisionInputValidator(),
            new UpdateSubmissionInputValidator());

    private static (AssignmentSystem.Domain.Entities.Assignment Assignment, User Student) SeedPublishedAssignmentWithEnrolledStudent(
        AssignmentSystem.Infrastructure.Persistence.AppDbContext dbContext, bool enrollStudent = true)
    {
        var classEntity = Class.Create("Grade 10 - A", null, Now).Value;
        var subjectEntity = Subject.Create("Mathematics", null, Now).Value;
        var teacher = User.Create("teacher@school.edu", "hashed", "Jane Smith", UserRole.Teacher, Now).Value;
        var student = User.Create("student@school.edu", "hashed", "John Doe", UserRole.Student, Now).Value;

        var assignment = AssignmentSystem.Domain.Entities.Assignment.Create(
            "Homework", "Description", classEntity.Id, subjectEntity.Id, teacher.Id, Now.AddDays(7), 100, Now).Value;
        assignment.Publish(Now);

        dbContext.Classes.Add(classEntity);
        dbContext.Subjects.Add(subjectEntity);
        dbContext.Users.AddRange(teacher, student);
        dbContext.Assignments.Add(assignment);

        if (enrollStudent)
        {
            dbContext.StudentEnrollments.Add(StudentEnrollment.Create(student.Id, classEntity.Id, Now));
        }

        dbContext.SaveChanges();

        return (assignment, student);
    }

    [Fact]
    public async Task Create_when_student_not_enrolled_should_fail()
    {
        using var dbContext = TestDbContextFactory.Create();
        var (assignment, student) = SeedPublishedAssignmentWithEnrolledStudent(dbContext, enrollStudent: false);

        var service = CreateService(dbContext);
        var result = await service.CreateAsync(
            new CreateSubmissionInput(student.Id, assignment.Id, "My answer", null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not enrolled");
    }

    [Fact]
    public async Task Create_for_draft_assignment_should_fail()
    {
        using var dbContext = TestDbContextFactory.Create();

        var classEntity = Class.Create("Grade 10 - A", null, Now).Value;
        var subjectEntity = Subject.Create("Mathematics", null, Now).Value;
        var teacher = User.Create("teacher@school.edu", "hashed", "Jane Smith", UserRole.Teacher, Now).Value;
        var student = User.Create("student@school.edu", "hashed", "John Doe", UserRole.Student, Now).Value;
        var draftAssignment = AssignmentSystem.Domain.Entities.Assignment.Create(
            "Homework", "Description", classEntity.Id, subjectEntity.Id, teacher.Id, Now.AddDays(7), 100, Now).Value;

        dbContext.Classes.Add(classEntity);
        dbContext.Subjects.Add(subjectEntity);
        dbContext.Users.AddRange(teacher, student);
        dbContext.Assignments.Add(draftAssignment);
        dbContext.StudentEnrollments.Add(StudentEnrollment.Create(student.Id, classEntity.Id, Now));
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var result = await service.CreateAsync(
            new CreateSubmissionInput(student.Id, draftAssignment.Id, "My answer", null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Create_valid_submission_should_succeed()
    {
        using var dbContext = TestDbContextFactory.Create();
        var (assignment, student) = SeedPublishedAssignmentWithEnrolledStudent(dbContext);

        var service = CreateService(dbContext);
        var result = await service.CreateAsync(
            new CreateSubmissionInput(student.Id, assignment.Id, "My answer", null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Submitted");
    }

    [Fact]
    public async Task Create_duplicate_submission_should_fail()
    {
        using var dbContext = TestDbContextFactory.Create();
        var (assignment, student) = SeedPublishedAssignmentWithEnrolledStudent(dbContext);

        var service = CreateService(dbContext);
        await service.CreateAsync(new CreateSubmissionInput(student.Id, assignment.Id, "My answer", null), CancellationToken.None);

        var result = await service.CreateAsync(
            new CreateSubmissionInput(student.Id, assignment.Id, "Second attempt", null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already submitted");
    }
}
