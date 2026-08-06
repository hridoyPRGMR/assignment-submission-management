using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AssignmentSystem.Api.Tests;

public class AuthWorkflowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthWorkflowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_with_valid_credentials_returns_tokens()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = CustomWebApplicationFactory.AdminEmail,
            password = CustomWebApplicationFactory.AdminPassword,
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<LoginResponseBody>();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();
        body.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Login_with_wrong_password_returns_400()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = CustomWebApplicationFactory.AdminEmail,
            password = "wrong-password",
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Accessing_protected_endpoint_without_token_returns_401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/users");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_token_rotation_invalidates_the_old_token()
    {
        var client = _factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = CustomWebApplicationFactory.AdminEmail,
            password = CustomWebApplicationFactory.AdminPassword,
        });

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponseBody>();

        var firstRefresh = await client.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken = loginBody!.RefreshToken });
        firstRefresh.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondRefreshWithSameToken = await client.PostAsJsonAsync(
            "/api/v1/auth/refresh", new { refreshToken = loginBody.RefreshToken });

        secondRefreshWithSameToken.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private record LoginResponseBody(string AccessToken, string RefreshToken, string Role);
}
