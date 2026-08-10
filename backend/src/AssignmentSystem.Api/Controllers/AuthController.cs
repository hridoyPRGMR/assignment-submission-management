using System.Security.Claims;
using AssignmentSystem.Api.Common;
using AssignmentSystem.Application.Features.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.RateLimiting;

namespace AssignmentSystem.Api.Controllers;

public class AuthController(IAuthService authService, IWebHostEnvironment environment, ILogger<AuthController> logger)
    : ApiControllerBase
{
    private const string RefreshTokenCookieName = "refreshToken";

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"),
            Email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email"),
            FullName = User.FindFirstValue(ClaimTypes.Name),
            Role = User.FindFirstValue(ClaimTypes.Role)
        });
    }

    [EnableRateLimiting("login")]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginInput input, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(input, cancellationToken);

        if (result.IsFailure)
        {
            logger.LogWarning("Login failed for {Email}: {Error}", input.Email, result.Error);
            return HandleResult(result);
        }

        logger.LogInformation("User {UserId} ({Email}) logged in", result.Value.UserId, result.Value.Email);
        SetRefreshTokenCookie(result.Value.RefreshToken);
        return HandleResult(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] RefreshTokenInput? input,
        CancellationToken cancellationToken)
    {
        var refreshToken = ResolveRefreshToken(input?.RefreshToken);
        var result = await authService.RefreshTokenAsync(new RefreshTokenInput(refreshToken), cancellationToken);

        if (result.IsSuccess)
        {
            SetRefreshTokenCookie(result.Value.RefreshToken);
        }

        return HandleResult(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] LogoutInput? input,
        CancellationToken cancellationToken)
    {
        var refreshToken = ResolveRefreshToken(input?.RefreshToken);
        var result = await authService.LogoutAsync(new LogoutInput(refreshToken), cancellationToken);
        ClearRefreshTokenCookie();
        return HandleResult(result);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var input = new ChangePasswordInput(CurrentUserId, request.CurrentPassword, request.NewPassword);
        var result = await authService.ChangePasswordAsync(input, cancellationToken);

        if (result.IsFailure)
        {
            logger.LogWarning("Password change failed for user {UserId}: {Error}", CurrentUserId, result.Error);
        }
        else
        {
            logger.LogInformation("User {UserId} changed their password", CurrentUserId);
        }

        return HandleResult(result);
    }

    private string ResolveRefreshToken(string? bodyValue) =>
        !string.IsNullOrWhiteSpace(bodyValue) ? bodyValue : Request.Cookies[RefreshTokenCookieName] ?? string.Empty;

    private void SetRefreshTokenCookie(string refreshToken)
    {
        Response.Cookies.Append(RefreshTokenCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(),
            // Frontend and API are deployed on different origins (e.g. Vercel + Render), so the
            // cookie must be sent cross-site. SameSite=None requires Secure=true, which is already
            // forced outside Development. Lax is kept for local dev where both run on localhost.
            SameSite = environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            Path = "/",
        });
    }

    private void ClearRefreshTokenCookie()
    {
        Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions { Path = "/" });
    }
}

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
