using AssignmentSystem.Domain.Entities;
using AssignmentSystem.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace AssignmentSystem.Domain.Tests.Entities;

public class SubmissionTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly Guid AssignmentId = Guid.NewGuid();
    private static readonly Guid StudentId = Guid.NewGuid();
    private static readonly Guid TeacherId = Guid.NewGuid();
    private const int MaxMarks = 100;

    private static Submission CreateValidSubmission(DateTime? deadline = null, DateTime? submittedAt = null) =>
        Submission.Create(
            AssignmentId, StudentId, "My answer", null, null, null, null,
            deadline ?? Now.AddDays(1), submittedAt ?? Now).Value;

    [Fact]
    public void Create_before_deadline_should_succeed_with_Submitted_status()
    {
        var result = Submission.Create(AssignmentId, StudentId, "My answer", null, null, null, null, Now.AddDays(1), Now);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(SubmissionStatus.Submitted);
        result.Value.IsLate.Should().BeFalse();
    }

    [Fact]
    public void Create_after_deadline_should_succeed_with_LateSubmission_status()
    {
        var deadline = Now.AddDays(-1);

        var result = Submission.Create(AssignmentId, StudentId, "My answer", null, null, null, null, deadline, Now);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(SubmissionStatus.LateSubmission);
        result.Value.IsLate.Should().BeTrue();
    }

    [Fact]
    public void Create_exactly_at_the_deadline_instant_should_not_be_late()
    {
        // IsLate is computed as `nowUtc > deadlineUtc`, so the exact same instant is on-time,
        // not late — this pins down the boundary explicitly rather than relying on a wide margin.
        var deadline = Now;

        var result = Submission.Create(AssignmentId, StudentId, "My answer", null, null, null, null, deadline, Now);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(SubmissionStatus.Submitted);
        result.Value.IsLate.Should().BeFalse();
    }

    [Fact]
    public void Create_one_tick_after_the_deadline_instant_should_be_late()
    {
        var deadline = Now;
        var submittedAt = Now.AddTicks(1);

        var result = Submission.Create(AssignmentId, StudentId, "My answer", null, null, null, null, deadline, submittedAt);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(SubmissionStatus.LateSubmission);
        result.Value.IsLate.Should().BeTrue();
    }

    [Fact]
    public void Create_with_empty_content_should_fail()
    {
        var result = Submission.Create(AssignmentId, StudentId, "   ", null, null, null, null, Now.AddDays(1), Now);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_with_attachment_should_store_file_metadata()
    {
        var result = Submission.Create(
            AssignmentId, StudentId, "My answer", "answer.pdf", "guid-123.pdf", "application/pdf", 2048,
            Now.AddDays(1), Now);

        result.IsSuccess.Should().BeTrue();
        result.Value.FileName.Should().Be("answer.pdf");
        result.Value.StoredFileName.Should().Be("guid-123.pdf");
        result.Value.FileContentType.Should().Be("application/pdf");
        result.Value.FileSizeBytes.Should().Be(2048);
    }

    [Fact]
    public void Update_before_deadline_while_Submitted_should_succeed()
    {
        var submission = CreateValidSubmission();

        var result = submission.Update(
            "Updated answer", replaceAttachment: false, null, null, null, null,
            Now.AddDays(1), AssignmentStatus.Published, Now);

        result.IsSuccess.Should().BeTrue();
        submission.Content.Should().Be("Updated answer");
    }

    [Fact]
    public void Update_after_deadline_should_succeed_and_mark_LateSubmission()
    {
        var submission = CreateValidSubmission(deadline: Now.AddHours(1));

        var result = submission.Update(
            "Updated answer", replaceAttachment: false, null, null, null, null,
            Now.AddHours(1), AssignmentStatus.Published, Now.AddHours(2));

        result.IsSuccess.Should().BeTrue();
        submission.Status.Should().Be(SubmissionStatus.LateSubmission);
        submission.IsLate.Should().BeTrue();
    }

    [Fact]
    public void Update_while_assignment_Graded_should_fail()
    {
        var submission = CreateValidSubmission();

        var result = submission.Update(
            "Updated answer", replaceAttachment: false, null, null, null, null,
            Now.AddDays(1), AssignmentStatus.Graded, Now);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Update_while_UnderReview_should_fail()
    {
        var submission = CreateValidSubmission();
        submission.StartReview(Now);

        var result = submission.Update(
            "Updated answer", replaceAttachment: false, null, null, null, null,
            Now.AddDays(1), AssignmentStatus.Published, Now);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Update_replacing_attachment_should_return_old_stored_file_name_for_cleanup()
    {
        var submission = Submission.Create(
            AssignmentId, StudentId, "My answer", "old.pdf", "old-stored.pdf", "application/pdf", 100,
            Now.AddDays(1), Now).Value;

        var result = submission.Update(
            "Updated answer", replaceAttachment: true, "new.pdf", "new-stored.pdf", "application/pdf", 200,
            Now.AddDays(1), AssignmentStatus.Published, Now);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("old-stored.pdf");
        submission.FileName.Should().Be("new.pdf");
        submission.StoredFileName.Should().Be("new-stored.pdf");
    }

    [Fact]
    public void Update_without_replacing_attachment_should_keep_existing_file_and_return_null()
    {
        var submission = Submission.Create(
            AssignmentId, StudentId, "My answer", "old.pdf", "old-stored.pdf", "application/pdf", 100,
            Now.AddDays(1), Now).Value;

        var result = submission.Update(
            "Updated answer", replaceAttachment: false, null, null, null, null,
            Now.AddDays(1), AssignmentStatus.Published, Now);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
        submission.FileName.Should().Be("old.pdf");
        submission.StoredFileName.Should().Be("old-stored.pdf");
    }

    [Fact]
    public void StartReview_from_Submitted_should_succeed()
    {
        var submission = CreateValidSubmission();

        var result = submission.StartReview(Now);

        result.IsSuccess.Should().BeTrue();
        submission.Status.Should().Be(SubmissionStatus.UnderReview);
    }

    [Fact]
    public void StartReview_from_LateSubmission_should_succeed()
    {
        var submission = Submission.Create(
            AssignmentId, StudentId, "My answer", null, null, null, null, Now.AddDays(-1), Now).Value;

        var result = submission.StartReview(Now);

        result.IsSuccess.Should().BeTrue();
        submission.Status.Should().Be(SubmissionStatus.UnderReview);
    }

    [Fact]
    public void StartReview_from_Graded_should_fail()
    {
        var submission = CreateValidSubmission();
        submission.StartReview(Now);
        submission.Grade(80, "Good work", MaxMarks, TeacherId, Now);

        var result = submission.StartReview(Now);

        result.IsFailure.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Grade_with_marks_outside_max_marks_range_should_fail(int marks)
    {
        var submission = CreateValidSubmission();

        var result = submission.Grade(marks, "Feedback", MaxMarks, TeacherId, Now);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("between 0 and");
    }

    [Fact]
    public void Grade_with_valid_marks_should_transition_to_Graded()
    {
        var submission = CreateValidSubmission();

        var result = submission.Grade(85, "Great job", MaxMarks, TeacherId, Now);

        result.IsSuccess.Should().BeTrue();
        submission.Status.Should().Be(SubmissionStatus.Graded);
        submission.Marks.Should().Be(85);
        submission.Feedback.Should().Be("Great job");
        submission.GradedByUserId.Should().Be(TeacherId);
    }

    [Fact]
    public void Grade_without_feedback_should_fail()
    {
        var submission = CreateValidSubmission();

        var result = submission.Grade(85, "", MaxMarks, TeacherId, Now);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Grade_a_Returned_submission_should_fail()
    {
        var submission = CreateValidSubmission();
        submission.StartReview(Now);
        submission.Return("Please redo section 2", Now);

        var result = submission.Grade(85, "Great job", MaxMarks, TeacherId, Now);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Return_from_UnderReview_should_succeed_and_clear_marks()
    {
        var submission = CreateValidSubmission();
        submission.StartReview(Now);

        var result = submission.Return("Please redo section 2", Now);

        result.IsSuccess.Should().BeTrue();
        submission.Status.Should().Be(SubmissionStatus.Returned);
        submission.Marks.Should().BeNull();
    }

    [Fact]
    public void Return_from_Submitted_should_fail()
    {
        var submission = CreateValidSubmission();

        var result = submission.Return("Please redo section 2", Now);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Full_state_machine_Submitted_to_UnderReview_to_Graded_to_Returned_to_Submitted()
    {
        var submission = CreateValidSubmission();

        submission.StartReview(Now).IsSuccess.Should().BeTrue();
        submission.Status.Should().Be(SubmissionStatus.UnderReview);

        submission.Grade(90, "Nicely done", MaxMarks, TeacherId, Now).IsSuccess.Should().BeTrue();
        submission.Status.Should().Be(SubmissionStatus.Graded);

        submission.Return("Actually, please expand on point 3", Now).IsSuccess.Should().BeTrue();
        submission.Status.Should().Be(SubmissionStatus.Returned);
        submission.Marks.Should().BeNull();

        submission.Update(
            "Expanded answer", replaceAttachment: false, null, null, null, null,
            Now.AddDays(1), AssignmentStatus.Published, Now).IsSuccess.Should().BeTrue();
        submission.Status.Should().Be(SubmissionStatus.Submitted);
    }
}
