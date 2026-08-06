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

    private record LoginResponseBody(string AccessToken, string RefreshToken, string Role);
}
