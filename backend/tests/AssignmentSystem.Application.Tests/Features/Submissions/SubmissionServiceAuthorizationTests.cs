using AssignmentSystem.Application.Features.Submissions;
using AssignmentSystem.Application.Tests.TestDoubles;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using AssignmentSystem.Infrastructure.Persistence;
using AssignmentSystem.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Xunit;

namespace AssignmentSystem.Application.Tests.Features.Submissions;

public class SubmissionServiceAuthorizationTests
{
    private static readonly DateTime Now = DateTime.UtcNow;

    private static SubmissionService CreateService(AppDbContext dbContext) =>
        new(
            new SubmissionRepository(dbContext),
            new AssignmentRepository(dbContext),
            new StudentEnrollmentRepository(dbContext),
            dbContext,
            new InMemoryFileStorageService(),
            new CreateSubmissionInputValidator(),
            new GradeSubmissionInputValidator(),
            new ReturnSubmissionForRevisionInputValidator(),
            new UpdateSubmissionInputValidator());

    private static (Submission Submission, Assignment Assignment, User OwningTeacher, User OtherTeacher, User Student, User OtherStudent)
        SeedSubmission(AppDbContext dbContext)
    {
        var classEntity = Class.Create("Grade 10 - A", null, Now).Value;
        var subjectEntity = Subject.Create("Mathematics", null, Now).Value;
        var owningTeacher = User.Create("teacher@school.edu", "hashed", "Jane Smith", UserRole.Teacher, Now).Value;
        var otherTeacher = User.Create("other.teacher@school.edu", "hashed", "Bob Jones", UserRole.Teacher, Now).Value;
        var student = User.Create("student@school.edu", "hashed", "John Doe", UserRole.Student, Now).Value;
        var otherStudent = User.Create("other.student@school.edu", "hashed", "Jane Roe", UserRole.Student, Now).Value;

        var assignment = Assignment.Create(
            "Homework", "Description", classEntity.Id, subjectEntity.Id, owningTeacher.Id, Now.AddDays(7), 100, Now).Value;
        assignment.Publish(Now);

        var submission = Submission.Create(
            assignment.Id, student.Id, "My answer", null, null, null, null, assignment.DeadlineUtc, Now).Value;

        dbContext.Classes.Add(classEntity);
        dbContext.Subjects.Add(subjectEntity);
        dbContext.Users.AddRange(owningTeacher, otherTeacher, student, otherStudent);
        dbContext.Assignments.Add(assignment);
        dbContext.Submissions.Add(submission);
        dbContext.SaveChanges();

        return (submission, assignment, owningTeacher, otherTeacher, student, otherStudent);
    }

    [Fact]
    public async Task UpdateAsync_by_non_owning_student_should_be_rejected()
    {
        using var dbContext = TestDbContextFactory.Create();
        var (submission, _, _, _, _, otherStudent) = SeedSubmission(dbContext);

        var service = CreateService(dbContext);

        var result = await service.UpdateAsync(
            new UpdateSubmissionInput(submission.Id, otherStudent.Id, "Someone else's edit", false, null, null, null, null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("do not own");
    }

    [Fact]
    public async Task StartReviewAsync_by_non_owning_teacher_should_be_rejected()
    {
        using var dbContext = TestDbContextFactory.Create();
        var (submission, _, _, otherTeacher, _, _) = SeedSubmission(dbContext);

        var service = CreateService(dbContext);

        var result = await service.StartReviewAsync(submission.Id, otherTeacher.Id, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("do not own");
    }

    [Fact]
    public async Task ReturnForRevisionAsync_by_non_owning_teacher_should_be_rejected()
    {
        using var dbContext = TestDbContextFactory.Create();
        var (submission, _, owningTeacher, otherTeacher, _, _) = SeedSubmission(dbContext);

        var service = CreateService(dbContext);
        await service.StartReviewAsync(submission.Id, owningTeacher.Id, CancellationToken.None);

        var result = await service.ReturnForRevisionAsync(
            new ReturnSubmissionForRevisionInput(submission.Id, otherTeacher.Id, "Please redo this"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("do not own");
    }

    [Fact]
    public async Task GetListForAssignmentAsync_by_non_owning_non_admin_teacher_should_be_rejected()
    {
        using var dbContext = TestDbContextFactory.Create();
        var (_, assignment, _, otherTeacher, _, _) = SeedSubmission(dbContext);

        var service = CreateService(dbContext);

        var result = await service.GetListForAssignmentAsync(assignment.Id, otherTeacher.Id, isAdmin: false, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("do not own");
    }

    [Fact]
    public async Task GetListForAssignmentAsync_by_admin_should_succeed_regardless_of_ownership()
    {
        using var dbContext = TestDbContextFactory.Create();
        var (_, assignment, _, otherTeacher, _, _) = SeedSubmission(dbContext);

        var service = CreateService(dbContext);

        var result = await service.GetListForAssignmentAsync(assignment.Id, otherTeacher.Id, isAdmin: true, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
    }
}
