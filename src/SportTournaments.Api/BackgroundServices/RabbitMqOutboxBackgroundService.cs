using Microsoft.Extensions.Options;
using SportTournaments.Application.Common.Outbox;

namespace SportTournaments.Api.BackgroundServices;

public sealed class RabbitMqOutboxBackgroundService(IServiceScopeFactory scopeFactory, IOptions<OutboxProcessingSettings> options,
    ILogger<RabbitMqOutboxBackgroundService> logger) : BackgroundService
{
    private static readonly string[] SupportedMessageTypes =
    [
        OutboxMessageTypes.TournamentCreated
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;

        logger.LogInformation("RabbitMQ Outbox background service started.");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                int claimedCount;

                try
                {
                    await using var scope = scopeFactory.CreateAsyncScope();

                    var processor = scope.ServiceProvider.GetRequiredService<IOutboxProcessor>();

                    claimedCount = await processor.ProcessBatchAsync(SupportedMessageTypes, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Unexpected error while processing RabbitMQ Outbox batch.");

                    await Task.Delay(settings.PollingInterval, stoppingToken);

                    continue;
                }

                if (claimedCount < settings.BatchSize)
                {
                    await Task.Delay(settings.PollingInterval, stoppingToken);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Expected during application shutdown.
        }
        finally
        {
            logger.LogInformation("RabbitMQ Outbox background service stopped.");
        }
    }
}