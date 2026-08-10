using AssignmentSystem.Application.Features.Assignments;
using AssignmentSystem.Application.Tests.TestDoubles;
using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using AssignmentSystem.Infrastructure.Persistence;
using AssignmentSystem.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Xunit;

namespace AssignmentSystem.Application.Tests.Features.Assignments;

public class DeadlineTransitionServiceTests
{
    private static readonly DateTime Now = DateTime.UtcNow;
    private static readonly Guid ClassId = Guid.NewGuid();
    private static readonly Guid SubjectId = Guid.NewGuid();
    private static readonly Guid TeacherId = Guid.NewGuid();

    private static DeadlineTransitionService CreateService(AppDbContext dbContext) =>
        new(
            new AssignmentRepository(dbContext),
            new SubmissionRepository(dbContext),
            new StudentEnrollmentRepository(dbContext),
            dbContext,
            new CapturingLogger<DeadlineTransitionService>());

    [Fact]
    public async Task ProcessDeadlinesAsync_marks_overdue_published_assignment_as_processed()
    {
        using var dbContext = TestDbContextFactory.Create();
        var classEntity = Class.Create("Grade 10 - A", null, Now).Value;
        var subject = Subject.Create("Mathematics", null, Now).Value;
        var assignment = Assignment.Create(
            "Homework", "Description", classEntity.Id, subject.Id, TeacherId, Now.AddHours(1), 100, Now).Value;
        assignment.Publish(Now);

        dbContext.Classes.Add(classEntity);
        dbContext.Subjects.Add(subject);
        dbContext.Assignments.Add(assignment);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var afterDeadline = Now.AddHours(2);

        var processedCount = await service.ProcessDeadlinesAsync(afterDeadline, CancellationToken.None);

        processedCount.Should().Be(1);
        var reloaded = dbContext.Assignments.Single(a => a.Id == assignment.Id);
        reloaded.DeadlineProcessedAtUtc.Should().Be(afterDeadline);
    }

    [Fact]
    public async Task ProcessDeadlinesAsync_skips_assignments_already_processed()
    {
        using var dbContext = TestDbContextFactory.Create();
        var classEntity = Class.Create("Grade 10 - A", null, Now).Value;
        var subject = Subject.Create("Mathematics", null, Now).Value;
        var assignment = Assignment.Create(
            "Homework", "Description", classEntity.Id, subject.Id, TeacherId, Now.AddHours(1), 100, Now).Value;
        assignment.Publish(Now);
        assignment.MarkDeadlineProcessed(Now.AddHours(2));

        dbContext.Classes.Add(classEntity);
        dbContext.Subjects.Add(subject);
        dbContext.Assignments.Add(assignment);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var processedCount = await service.ProcessDeadlinesAsync(Now.AddHours(3), CancellationToken.None);

        processedCount.Should().Be(0);
    }

    [Fact]
    public async Task ProcessDeadlinesAsync_skips_assignments_not_yet_overdue()
    {
        using var dbContext = TestDbContextFactory.Create();
        var classEntity = Class.Create("Grade 10 - A", null, Now).Value;
        var subject = Subject.Create("Mathematics", null, Now).Value;
        var assignment = Assignment.Create(
            "Homework", "Description", classEntity.Id, subject.Id, TeacherId, Now.AddDays(7), 100, Now).Value;
        assignment.Publish(Now);

        dbContext.Classes.Add(classEntity);
        dbContext.Subjects.Add(subject);
        dbContext.Assignments.Add(assignment);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var processedCount = await service.ProcessDeadlinesAsync(Now, CancellationToken.None);

        processedCount.Should().Be(0);
    }

    [Fact]
    public async Task ProcessDeadlinesAsync_skips_draft_assignments_even_if_overdue()
    {
        using var dbContext = TestDbContextFactory.Create();
        var classEntity = Class.Create("Grade 10 - A", null, Now).Value;
        var subject = Subject.Create("Mathematics", null, Now).Value;
        var assignment = Assignment.Create(
            "Homework", "Description", classEntity.Id, subject.Id, TeacherId, Now.AddHours(1), 100, Now).Value;

        dbContext.Classes.Add(classEntity);
        dbContext.Subjects.Add(subject);
        dbContext.Assignments.Add(assignment);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var processedCount = await service.ProcessDeadlinesAsync(Now.AddHours(2), CancellationToken.None);

        processedCount.Should().Be(0);
    }

    [Fact]
    public async Task ProcessDeadlinesAsync_skips_graded_assignments_even_if_overdue()
    {
        using var dbContext = TestDbContextFactory.Create();
        var classEntity = Class.Create("Grade 10 - A", null, Now).Value;
        var subject = Subject.Create("Mathematics", null, Now).Value;
        var assignment = Assignment.Create(
            "Homework", "Description", classEntity.Id, subject.Id, TeacherId, Now.AddHours(1), 100, Now).Value;
        assignment.Publish(Now);
        assignment.MarkGraded(Now.AddHours(2));

        dbContext.Classes.Add(classEntity);
        dbContext.Subjects.Add(subject);
        dbContext.Assignments.Add(assignment);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var processedCount = await service.ProcessDeadlinesAsync(Now.AddHours(3), CancellationToken.None);

        processedCount.Should().Be(0);
    }

    [Fact]
    public async Task ProcessDeadlinesAsync_counts_late_and_total_submissions_correctly()
    {
        using var dbContext = TestDbContextFactory.Create();
        var classEntity = Class.Create("Grade 10 - A", null, Now).Value;
        var subject = Subject.Create("Mathematics", null, Now).Value;
        var onTimeStudent = User.Create("student1@school.edu", "hashed", "Student One", UserRole.Student, Now).Value;
        var lateStudent = User.Create("student2@school.edu", "hashed", "Student Two", UserRole.Student, Now).Value;

        var assignment = Assignment.Create(
            "Homework", "Description", classEntity.Id, subject.Id, TeacherId, Now.AddHours(1), 100, Now).Value;
        assignment.Publish(Now);

        var onTimeSubmission = Submission.Create(
            assignment.Id, onTimeStudent.Id, "Answer", null, null, null, null, assignment.DeadlineUtc, Now.AddMinutes(30)).Value;
        var lateSubmission = Submission.Create(
            assignment.Id, lateStudent.Id, "Answer", null, null, null, null, assignment.DeadlineUtc, Now.AddHours(2)).Value;

        dbContext.Classes.Add(classEntity);
        dbContext.Subjects.Add(subject);
        dbContext.Users.AddRange(onTimeStudent, lateStudent);
        dbContext.Assignments.Add(assignment);
        dbContext.Submissions.AddRange(onTimeSubmission, lateSubmission);
        dbContext.StudentEnrollments.AddRange(
            StudentEnrollment.Create(onTimeStudent.Id, classEntity.Id, Now),
            StudentEnrollment.Create(lateStudent.Id, classEntity.Id, Now));
        await dbContext.SaveChangesAsync();

        var logger = new CapturingLogger<DeadlineTransitionService>();
        var service = new DeadlineTransitionService(
            new AssignmentRepository(dbContext),
            new SubmissionRepository(dbContext),
            new StudentEnrollmentRepository(dbContext),
            dbContext,
            logger);

        var processedCount = await service.ProcessDeadlinesAsync(Now.AddHours(3), CancellationToken.None);

        processedCount.Should().Be(1);
        logger.Entries.Should().Contain(e => e.Message.Contains("1") && e.Message.Contains(assignment.Id.ToString()));
    }
}
