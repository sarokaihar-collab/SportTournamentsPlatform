using SportTournaments.NotificationService.Email;
using SportTournaments.NotificationService.Persistence.Repositories;

namespace SportTournaments.NotificationService.BackgroundServices;

public sealed class EmailDeliveryWorker(IServiceScopeFactory scopeFactory, ILogger<EmailDeliveryWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Email delivery worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "An error occurred while processing email deliveries.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        logger.LogInformation("Email delivery worker stopped.");
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var repository = scope.ServiceProvider.GetRequiredService<IEmailDeliveryRepository>();

        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        var deliveries = await repository.ClaimBatchAsync(batchSize: 10, lockDuration: TimeSpan.FromMinutes(2), cancellationToken);

        logger.LogInformation("Claimed {Count} email deliveries.", deliveries.Count);

        foreach (var delivery in deliveries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await emailSender.SendAsync(delivery.RecipientEmail, delivery.Subject, delivery.Body, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to send email delivery {EmailDeliveryId}.", delivery.Id);

                await repository.MarkAsFailedAsync(
                    delivery.Id,
                    delivery.LockToken!.Value,
                    exception.Message,
                    retryDelay: TimeSpan.FromMinutes(1),
                    maxAttempts: 5,
                    cancellationToken);

                continue;
            }

            await repository.MarkAsSentAsync(delivery.Id, delivery.LockToken!.Value, DateTimeOffset.UtcNow, cancellationToken);

            logger.LogInformation("Email delivery {EmailDeliveryId} sent successfully.", delivery.Id);
        }
    }
}