using System.Security.Claims;
using AssignmentSystem.Api.Common;
using AssignmentSystem.Application.Features.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

public class AuthController(IAuthService authService) : ApiControllerBase
{
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

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginInput input, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(input, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenInput input, CancellationToken cancellationToken)
    {
        var result = await authService.RefreshTokenAsync(input, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutInput input, CancellationToken cancellationToken)
    {
        var result = await authService.LogoutAsync(input, cancellationToken);
        return HandleResult(result);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var input = new ChangePasswordInput(CurrentUserId, request.CurrentPassword, request.NewPassword);
        var result = await authService.ChangePasswordAsync(input, cancellationToken);
        return HandleResult(result);
    }
}

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
