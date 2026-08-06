using AssignmentSystem.Application.Features.Submissions;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using AssignmentSystem.Infrastructure.Persistence;
using FluentAssertions;
using Xunit;

namespace AssignmentSystem.Application.Tests.Features.Submissions;

public class GradeSubmissionCommandHandlerTests
{
    private static readonly DateTime Now = DateTime.UtcNow;

    private static (Submission Submission, User OwningTeacher, User OtherTeacher) SeedGradeableSubmission(AppDbContext dbContext)
    {
        var classEntity = Class.Create("Grade 10 - A", null, Now).Value;
        var subjectEntity = Subject.Create("Mathematics", null, Now).Value;
        var owningTeacher = User.Create("teacher@school.edu", "hashed", "Jane Smith", UserRole.Teacher, Now).Value;
        var otherTeacher = User.Create("other.teacher@school.edu", "hashed", "Bob Jones", UserRole.Teacher, Now).Value;
        var student = User.Create("student@school.edu", "hashed", "John Doe", UserRole.Student, Now).Value;

        var assignment = AssignmentSystem.Domain.Entities.Assignment.Create(
            "Homework", "Description", classEntity.Id, subjectEntity.Id, owningTeacher.Id, Now.AddDays(7), 100, Now).Value;
        assignment.Publish();

        var submission = Submission.Create(assignment.Id, student.Id, "My answer", null, assignment.DeadlineUtc, Now).Value;

        dbContext.Classes.Add(classEntity);
        dbContext.Subjects.Add(subjectEntity);
        dbContext.Users.AddRange(owningTeacher, otherTeacher, student);
        dbContext.Assignments.Add(assignment);
        dbContext.Submissions.Add(submission);
        dbContext.SaveChanges();

        return (submission, owningTeacher, otherTeacher);
    }

    [Fact]
    public async Task Grade_by_non_owning_teacher_should_be_rejected()
    {
        using var dbContext = TestDbContextFactory.Create();
        var (submission, _, otherTeacher) = SeedGradeableSubmission(dbContext);

        var handler = new GradeSubmissionCommandHandler(dbContext);

        var result = await handler.Handle(
            new GradeSubmissionCommand(submission.Id, otherTeacher.Id, 80, "Nice work"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("do not own");
        submission.Status.Should().Be(SubmissionStatus.Submitted);
    }

    [Fact]
    public async Task Grade_by_owning_teacher_should_succeed()
    {
        using var dbContext = TestDbContextFactory.Create();
        var (submission, owningTeacher, _) = SeedGradeableSubmission(dbContext);

        var handler = new GradeSubmissionCommandHandler(dbContext);

        var result = await handler.Handle(
            new GradeSubmissionCommand(submission.Id, owningTeacher.Id, 80, "Nice work"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Graded");
        result.Value.Marks.Should().Be(80);
    }

    [Fact]
    public async Task Grade_with_marks_exceeding_max_marks_should_fail()
    {
        using var dbContext = TestDbContextFactory.Create();
        var (submission, owningTeacher, _) = SeedGradeableSubmission(dbContext);

        var handler = new GradeSubmissionCommandHandler(dbContext);

        var result = await handler.Handle(
            new GradeSubmissionCommand(submission.Id, owningTeacher.Id, 150, "Nice work"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}
