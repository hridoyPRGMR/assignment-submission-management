using AssignmentSystem.Application.Features.Assignments;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AssignmentSystem.Infrastructure.BackgroundServices;

public class DeadlineProcessingService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<DeadlineProcessingService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalSeconds = configuration.GetValue("DeadlineProcessing:PollingIntervalSeconds", 60);
        var interval = TimeSpan.FromSeconds(intervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var deadlineTransitionService = scope.ServiceProvider.GetRequiredService<IDeadlineTransitionService>();
                var processedCount = await deadlineTransitionService.ProcessDeadlinesAsync(DateTime.UtcNow, stoppingToken);

                if (processedCount > 0)
                {
                    logger.LogInformation("Deadline processing sweep handled {ProcessedCount} assignment(s)", processedCount);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Deadline processing sweep failed");
            }

            try
            {
                await Task.Delay(interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
