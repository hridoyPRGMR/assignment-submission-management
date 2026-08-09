using AssignmentSystem.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentSystem.Api.Common;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            return NoContent();
        }

        return MapFailure(result.ErrorType, result.Error!);
    }

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return MapFailure(result.ErrorType, result.Error!);
    }

    private IActionResult MapFailure(ErrorType errorType, string error) => errorType switch
    {
        ErrorType.NotFound => Problem(statusCode: StatusCodes.Status404NotFound, title: error),
        ErrorType.Forbidden => Problem(statusCode: StatusCodes.Status403Forbidden, title: error),
        _ => Problem(statusCode: StatusCodes.Status400BadRequest, title: error),
    };
}
