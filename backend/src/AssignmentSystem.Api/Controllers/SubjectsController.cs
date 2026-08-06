using AssignmentSystem.Api.Common;
using AssignmentSystem.Application.Features.Subjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Controllers;

[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class SubjectsController(ISubjectService subjectService) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSubjects(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await subjectService.GetListAsync(page, pageSize, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSubjectById(Guid id, CancellationToken cancellationToken)
    {
        var result = await subjectService.GetByIdAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSubject(CreateSubjectInput input, CancellationToken cancellationToken)
    {
        var result = await subjectService.CreateAsync(input, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateSubject(Guid id, UpdateSubjectRequest request, CancellationToken cancellationToken)
    {
        var result = await subjectService.UpdateAsync(new UpdateSubjectInput(id, request.Name, request.Description), cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteSubject(Guid id, CancellationToken cancellationToken)
    {
        var result = await subjectService.DeleteAsync(id, cancellationToken);
        return HandleResult(result);
    }
}

public record UpdateSubjectRequest(string Name, string? Description);
