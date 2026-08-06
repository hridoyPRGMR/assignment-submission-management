using System.Diagnostics;
using AssignmentSystem.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AssignmentSystem.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        var response = await next(cancellationToken);

        stopwatch.Stop();

        if (response is Result { IsFailure: true } result)
        {
            logger.LogWarning(
                "Request {RequestName} failed in {ElapsedMs}ms: {Error}",
                requestName, stopwatch.ElapsedMilliseconds, result.Error);
        }
        else
        {
            logger.LogInformation(
                "Request {RequestName} handled in {ElapsedMs}ms",
                requestName, stopwatch.ElapsedMilliseconds);
        }

        return response;
    }
}
