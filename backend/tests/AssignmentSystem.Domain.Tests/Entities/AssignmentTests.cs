using AssignmentSystem.Domain.Enums;
using FluentAssertions;
using Xunit;
using DomainAssignment = AssignmentSystem.Domain.Entities.Assignment;

namespace AssignmentSystem.Domain.Tests.Entities;

public class AssignmentTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly Guid ClassId = Guid.NewGuid();
    private static readonly Guid SubjectId = Guid.NewGuid();
    private static readonly Guid TeacherId = Guid.NewGuid();

    private static DomainAssignment CreateValidAssignment(int maxMarks = 100, DateTime? deadline = null) =>
        DomainAssignment.Create(
            "Algebra Homework",
            "Solve problems 1-10",
            ClassId,
            SubjectId,
            TeacherId,
            deadline ?? Now.AddDays(7),
            maxMarks,
            Now).Value;

    [Fact]
    public void Create_with_valid_data_should_default_to_draft()
    {
        var result = DomainAssignment.Create(
            "Algebra Homework", "Description", ClassId, SubjectId, TeacherId, Now.AddDays(1), 100, Now);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(AssignmentStatus.Draft);
        result.Value.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Create_with_deadline_in_the_past_should_fail()
    {
        var result = DomainAssignment.Create(
            "Algebra Homework", "Description", ClassId, SubjectId, TeacherId, Now.AddDays(-1), 100, Now);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("future");
    }

    [Fact]
    public void Create_with_zero_or_negative_max_marks_should_fail()
    {
        var result = DomainAssignment.Create(
            "Algebra Homework", "Description", ClassId, SubjectId, TeacherId, Now.AddDays(1), 0, Now);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Maximum marks");
    }

    [Fact]
    public void Publish_a_draft_assignment_should_succeed()
    {
        var assignment = CreateValidAssignment();

        var result = assignment.Publish(Now);

        result.IsSuccess.Should().BeTrue();
        assignment.Status.Should().Be(AssignmentStatus.Published);
    }

    [Fact]
    public void Publish_an_already_published_assignment_should_fail()
    {
        var assignment = CreateValidAssignment();
        assignment.Publish(Now);

        var result = assignment.Publish(Now);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Unpublish_a_draft_assignment_should_fail()
    {
        var assignment = CreateValidAssignment();

        var result = assignment.Unpublish(Now);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void UpdateDetails_without_existing_submissions_can_change_deadline_and_marks()
    {
        var assignment = CreateValidAssignment();
        var newDeadline = Now.AddDays(14);

        var result = assignment.UpdateDetails("New title", "New description", newDeadline, 50, hasSubmissions: false, Now);

        result.IsSuccess.Should().BeTrue();
        assignment.Title.Should().Be("New title");
        assignment.DeadlineUtc.Should().Be(newDeadline);
        assignment.MaxMarks.Should().Be(50);
    }

    [Fact]
    public void UpdateDetails_with_existing_submissions_cannot_change_deadline_or_marks()
    {
        var deadline = Now.AddDays(7);
        var assignment = CreateValidAssignment(maxMarks: 100, deadline: deadline);

        var result = assignment.UpdateDetails("New title", "New description", deadline.AddDays(1), 50, hasSubmissions: true, Now);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("cannot be changed");
    }

    [Fact]
    public void UpdateDetails_with_existing_submissions_can_still_change_title_and_description()
    {
        var deadline = Now.AddDays(7);
        var assignment = CreateValidAssignment(maxMarks: 100, deadline: deadline);

        var result = assignment.UpdateDetails("New title", "New description", deadline, 100, hasSubmissions: true, Now);

        result.IsSuccess.Should().BeTrue();
        assignment.Title.Should().Be("New title");
    }

    [Fact]
    public void MarkDeleted_should_soft_delete()
    {
        var assignment = CreateValidAssignment();

        assignment.MarkDeleted(Now);

        assignment.IsDeleted.Should().BeTrue();
    }
}
