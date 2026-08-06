using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Xunit;

namespace AssignmentSystem.Api.Tests;

public class SubmissionFileUploadTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public SubmissionFileUploadTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<(HttpClient TeacherClient, HttpClient StudentClient, Guid AssignmentId)> CreateAndPublishAssignmentAsync()
    {
        var teacherClient = await _factory.CreateClient()
            .AuthenticateAsync(CustomWebApplicationFactory.TeacherEmail, CustomWebApplicationFactory.TeacherPassword);

        var createResponse = await teacherClient.PostAsJsonAsync("/api/v1/assignments", new
        {
            title = "Upload Test Assignment",
            description = "Solve the problems",
            classId = _factory.ClassId,
            subjectId = _factory.SubjectId,
            deadlineUtc = DateTime.UtcNow.AddDays(7),
            maxMarks = 100,
        });

        var assignment = await createResponse.Content.ReadFromJsonAsync<AssignmentBody>();
        await teacherClient.PostAsync($"/api/v1/assignments/{assignment!.Id}/publish", null);

        var studentClient = await _factory.CreateClient()
            .AuthenticateAsync(CustomWebApplicationFactory.StudentEmail, CustomWebApplicationFactory.StudentPassword);

        return (teacherClient, studentClient, assignment.Id);
    }

    [Fact]
    public async Task Submitting_with_an_allowed_pdf_attachment_succeeds_and_can_be_downloaded()
    {
        var (_, studentClient, assignmentId) = await CreateAndPublishAssignmentAsync();

        var submitResponse = await studentClient.PostSubmissionAsync(
            assignmentId, "Here is my answer", Encoding.UTF8.GetBytes("%PDF-1.4 fake pdf content"), "answer.pdf", "application/pdf");

        submitResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var submission = await submitResponse.Content.ReadFromJsonAsync<SubmissionFileBody>();
        submission!.FileName.Should().Be("answer.pdf");
        submission.FileDownloadUrl.Should().NotBeNull();

        var downloadResponse = await studentClient.GetAsync(submission.FileDownloadUrl);
        downloadResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var downloadedBytes = await downloadResponse.Content.ReadAsByteArrayAsync();
        Encoding.UTF8.GetString(downloadedBytes).Should().Contain("fake pdf content");
    }

    [Fact]
    public async Task Submitting_with_a_disallowed_file_extension_is_rejected()
    {
        var (_, studentClient, assignmentId) = await CreateAndPublishAssignmentAsync();

        var submitResponse = await studentClient.PostSubmissionAsync(
            assignmentId, "Here is my answer", Encoding.UTF8.GetBytes("malicious"), "answer.exe", "application/octet-stream");

        submitResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Another_students_submission_file_cannot_be_downloaded()
    {
        var (_, studentClient, assignmentId) = await CreateAndPublishAssignmentAsync();

        var submitResponse = await studentClient.PostSubmissionAsync(
            assignmentId, "Here is my answer", Encoding.UTF8.GetBytes("content"), "answer.pdf", "application/pdf");
        var submission = await submitResponse.Content.ReadFromJsonAsync<SubmissionFileBody>();

        var otherStudentClient = await _factory.CreateClient()
            .AuthenticateAsync(CustomWebApplicationFactory.OtherStudentEmail, CustomWebApplicationFactory.OtherStudentPassword);

        var downloadResponse = await otherStudentClient.GetAsync(submission!.FileDownloadUrl);

        downloadResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private record AssignmentBody(Guid Id, string Status);

    private record SubmissionFileBody(Guid Id, string? FileName, string? FileDownloadUrl);
}
