using System.Security.Claims;
using AssignmentSystem.Api.Common;
using AssignmentSystem.Application.Features.Users;
using AssignmentSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class UsersController(IUserService userService) : ApiControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] UserRole? role = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await userService.GetListAsync(page, pageSize, role, search, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        var result = await userService.GetByIdAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserInput input, CancellationToken cancellationToken)
    {
        var result = await userService.CreateAsync(input, CurrentUserId, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await userService.UpdateAsync(new UpdateUserInput(id, request.FullName, request.Role), CurrentUserId, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        var result = await userService.DeleteAsync(id, CurrentUserId, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await userService.SetActiveStatusAsync(id, true, CurrentUserId, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await userService.SetActiveStatusAsync(id, false, CurrentUserId, cancellationToken);
        return HandleResult(result);
    }
}

public record UpdateUserRequest(string FullName, UserRole? Role);
