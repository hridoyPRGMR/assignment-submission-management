using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AssignmentSystem.Api.Tests;

public class AssignmentWorkflowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AssignmentWorkflowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Full_workflow_create_publish_submit_grade_succeeds()
    {
        var teacherClient = await _factory.CreateClient()
            .AuthenticateAsync(CustomWebApplicationFactory.TeacherEmail, CustomWebApplicationFactory.TeacherPassword);

        var createResponse = await teacherClient.PostAsJsonAsync("/api/v1/assignments", new
        {
            title = "Integration Test Assignment",
            description = "Solve the problems",
            classId = _factory.ClassId,
            subjectId = _factory.SubjectId,
            deadlineUtc = DateTime.UtcNow.AddDays(7),
            maxMarks = 100,
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdAssignment = await createResponse.Content.ReadFromJsonAsync<AssignmentBody>();
        createdAssignment!.Status.Should().Be("Draft");

        var publishResponse = await teacherClient.PostAsync($"/api/v1/assignments/{createdAssignment.Id}/publish", null);
        publishResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var studentClient = await _factory.CreateClient()
            .AuthenticateAsync(CustomWebApplicationFactory.StudentEmail, CustomWebApplicationFactory.StudentPassword);

        var submitResponse = await studentClient.PostSubmissionAsync(createdAssignment.Id, "Here is my answer");

        submitResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var submission = await submitResponse.Content.ReadFromJsonAsync<SubmissionBody>();
        submission!.Status.Should().Be("Submitted");

        var gradeResponse = await teacherClient.PostAsJsonAsync($"/api/v1/submissions/{submission.Id}/grade", new
        {
            marks = 90,
            feedback = "Well done",
        });

        gradeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var gradedSubmission = await gradeResponse.Content.ReadFromJsonAsync<SubmissionBody>();
        gradedSubmission!.Status.Should().Be("Graded");
        gradedSubmission.Marks.Should().Be(90);
    }

    [Fact]
    public async Task Full_workflow_publish_deadline_pass_mark_graded_archive_then_edit_is_blocked()
    {
        var teacherClient = await _factory.CreateClient()
            .AuthenticateAsync(CustomWebApplicationFactory.TeacherEmail, CustomWebApplicationFactory.TeacherPassword);

        var createResponse = await teacherClient.PostAsJsonAsync("/api/v1/assignments", new
        {
            title = "Short Deadline Assignment",
            description = "Solve the problems",
            classId = _factory.ClassId,
            subjectId = _factory.SubjectId,
            deadlineUtc = DateTime.UtcNow.AddSeconds(1),
            maxMarks = 100,
        });

        var createdAssignment = await createResponse.Content.ReadFromJsonAsync<AssignmentBody>();

        await teacherClient.PostAsync($"/api/v1/assignments/{createdAssignment!.Id}/publish", null);

        await Task.Delay(1200);

        var markGradedResponse = await teacherClient.PostAsync($"/api/v1/assignments/{createdAssignment.Id}/mark-graded", null);
        markGradedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var gradedAssignment = await markGradedResponse.Content.ReadFromJsonAsync<AssignmentBody>();
        gradedAssignment!.Status.Should().Be("Graded");

        var editResponse = await teacherClient.PutAsJsonAsync($"/api/v1/assignments/{createdAssignment.Id}", new
        {
            title = "Should not be allowed",
            description = "Solve the problems",
            deadlineUtc = DateTime.UtcNow.AddDays(7),
            maxMarks = 100,
        });
        editResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var archiveResponse = await teacherClient.PostAsync($"/api/v1/assignments/{createdAssignment.Id}/archive", null);
        archiveResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var archivedAssignment = await archiveResponse.Content.ReadFromJsonAsync<AssignmentBody>();
        archivedAssignment!.Status.Should().Be("Archived");
    }

    [Fact]
    public async Task Submission_after_deadline_is_marked_late()
    {
        var teacherClient = await _factory.CreateClient()
            .AuthenticateAsync(CustomWebApplicationFactory.TeacherEmail, CustomWebApplicationFactory.TeacherPassword);

        var createResponse = await teacherClient.PostAsJsonAsync("/api/v1/assignments", new
        {
            title = "Late Submission Assignment",
            description = "Solve the problems",
            classId = _factory.ClassId,
            subjectId = _factory.SubjectId,
            deadlineUtc = DateTime.UtcNow.AddSeconds(1),
            maxMarks = 100,
        });

        var createdAssignment = await createResponse.Content.ReadFromJsonAsync<AssignmentBody>();
        await teacherClient.PostAsync($"/api/v1/assignments/{createdAssignment!.Id}/publish", null);

        await Task.Delay(1200);

        var studentClient = await _factory.CreateClient()
            .AuthenticateAsync(CustomWebApplicationFactory.StudentEmail, CustomWebApplicationFactory.StudentPassword);

        var submitResponse = await studentClient.PostSubmissionAsync(createdAssignment.Id, "Sorry this is late");

        submitResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var submission = await submitResponse.Content.ReadFromJsonAsync<SubmissionBody>();
        submission!.Status.Should().Be("LateSubmission");
    }

    [Fact]
    public async Task Student_cannot_create_assignment()
    {
        var studentClient = await _factory.CreateClient()
            .AuthenticateAsync(CustomWebApplicationFactory.StudentEmail, CustomWebApplicationFactory.StudentPassword);

        var response = await studentClient.PostAsJsonAsync("/api/v1/assignments", new
        {
            title = "Should not be created",
            description = "N/A",
            classId = _factory.ClassId,
            subjectId = _factory.SubjectId,
            deadlineUtc = DateTime.UtcNow.AddDays(7),
            maxMarks = 100,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Teacher_cannot_publish_another_teachers_assignment()
    {
        var otherTeacherClient = await _factory.CreateClient()
            .AuthenticateAsync(CustomWebApplicationFactory.OtherTeacherEmail, CustomWebApplicationFactory.OtherTeacherPassword);

        var response = await otherTeacherClient.PostAsync(
            $"/api/v1/assignments/{_factory.DraftAssignmentId}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Student_cannot_see_draft_assignment()
    {
        var studentClient = await _factory.CreateClient()
            .AuthenticateAsync(CustomWebApplicationFactory.StudentEmail, CustomWebApplicationFactory.StudentPassword);

        var response = await studentClient.GetAsync($"/api/v1/assignments/{_factory.DraftAssignmentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Student_can_see_published_assignment()
    {
        var studentClient = await _factory.CreateClient()
            .AuthenticateAsync(CustomWebApplicationFactory.StudentEmail, CustomWebApplicationFactory.StudentPassword);

        var response = await studentClient.GetAsync($"/api/v1/assignments/{_factory.PublishedAssignmentId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private record AssignmentBody(Guid Id, string Status);

    private record SubmissionBody(Guid Id, string Status, int? Marks);
}
