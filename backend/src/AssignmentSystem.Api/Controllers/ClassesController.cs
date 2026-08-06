using AssignmentSystem.Api.Common;
using AssignmentSystem.Application.Features.Classes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class ClassesController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet]
    public async Task<IActionResult> GetClasses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetClassesQuery(page, pageSize), cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetClassById(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetClassByIdQuery(id), cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateClass(CreateClassCommand command, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateClass(Guid id, UpdateClassRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new UpdateClassCommand(id, request.Name, request.Description), cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteClass(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new DeleteClassCommand(id), cancellationToken);
        return HandleResult(result);
    }
}

public record UpdateClassRequest(string Name, string? Description);
