using SportTournaments.NotificationService.Persistence.Entities;

namespace SportTournaments.NotificationService.Persistence.Repositories;

public interface IEmailDeliveryRepository
{
    Task<IReadOnlyList<EmailDelivery>> ClaimBatchAsync(int batchSize, TimeSpan lockDuration, CancellationToken cancellationToken = default);

    Task MarkAsSentAsync(Guid emailDeliveryId, Guid lockToken, DateTimeOffset sentAtUtc, CancellationToken cancellationToken = default);

    Task MarkAsFailedAsync(Guid emailDeliveryId, Guid lockToken, string error, TimeSpan retryDelay, int maxAttempts, CancellationToken cancellationToken = default);
}