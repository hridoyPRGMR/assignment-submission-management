using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Common;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class ApiControllerBase(ISender sender) : ControllerBase
{
    protected ISender Sender { get; } = sender;

    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            return NoContent();
        }

        return Problem(statusCode: StatusCodes.Status400BadRequest, title: result.Error);
    }

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return Problem(statusCode: StatusCodes.Status400BadRequest, title: result.Error);
    }
}
