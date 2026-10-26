using Microsoft.Extensions.Options;
using SportTournaments.Application.Common.Outbox;

namespace SportTournaments.Api.BackgroundServices;

public sealed class OutboxBackgroundService(IServiceScopeFactory scopeFactory, IOptions<OutboxProcessingSettings> options,
    ILogger<OutboxBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;

        logger.LogInformation("Outbox background service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                int claimedCount;

                // OutboxProcessor and its dependencies are scoped.
                // Create a fresh scope for each processing cycle.
                using (var scope = scopeFactory.CreateScope())
                {
                    var processor = scope.ServiceProvider.GetRequiredService<IOutboxProcessor>();
                    claimedCount = await processor.ProcessBatchAsync(stoppingToken);
                }

                // The scope is disposed before waiting for the next poll.
                // If we claimed a full batch, immediately check for more.
                if (claimedCount < settings.BatchSize)
                {
                    await Task.Delay(settings.PollingInterval, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "An unexpected error occurred during Outbox processing.");

                // Prevent a tight error loop if the database or another
                // dependency remains unavailable.
                try
                {
                    await Task.Delay(settings.PollingInterval, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        logger.LogInformation("Outbox background service stopped.");
    }
}