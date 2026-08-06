using System.Security.Claims;
using AssignmentSystem.Api.Common;
using AssignmentSystem.Application.Features.Classes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class ClassesController(IClassService classService) : ApiControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet]
    public async Task<IActionResult> GetClasses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await classService.GetListAsync(page, pageSize, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetClassById(Guid id, CancellationToken cancellationToken)
    {
        var result = await classService.GetByIdAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateClass(CreateClassInput input, CancellationToken cancellationToken)
    {
        var result = await classService.CreateAsync(input, CurrentUserId, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateClass(Guid id, UpdateClassRequest request, CancellationToken cancellationToken)
    {
        var result = await classService.UpdateAsync(
            new UpdateClassInput(id, request.Name, request.Description), CurrentUserId, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteClass(Guid id, CancellationToken cancellationToken)
    {
        var result = await classService.DeleteAsync(id, cancellationToken);
        return HandleResult(result);
    }
}

public record UpdateClassRequest(string Name, string? Description);
