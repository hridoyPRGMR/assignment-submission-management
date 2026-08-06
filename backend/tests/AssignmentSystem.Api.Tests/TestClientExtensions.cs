using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AssignmentSystem.Api.Tests;

public static class TestClientExtensions
{
    public static async Task<HttpClient> AuthenticateAsync(this HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<LoginResponseBody>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.AccessToken);

        return client;
    }

    public static Task<HttpResponseMessage> PostSubmissionAsync(
        this HttpClient client, Guid assignmentId, string content,
        byte[]? fileBytes = null, string? fileName = null, string? fileContentType = null)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(assignmentId.ToString()), "assignmentId" },
            { new StringContent(content), "content" }
        };

        if (fileBytes is not null)
        {
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(fileContentType ?? "application/octet-stream");
            form.Add(fileContent, "file", fileName ?? "attachment.bin");
        }

        return client.PostAsync("/api/v1/submissions", form);
    }

    private record LoginResponseBody(string AccessToken, string RefreshToken, string Role);
}
