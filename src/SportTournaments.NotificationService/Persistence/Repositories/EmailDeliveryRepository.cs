using Microsoft.EntityFrameworkCore;
using SportTournaments.NotificationService.Persistence.Entities;

namespace SportTournaments.NotificationService.Persistence.Repositories;

public sealed class EmailDeliveryRepository(NotificationDbContext dbContext) : IEmailDeliveryRepository
{
    public async Task<IReadOnlyList<EmailDelivery>> ClaimBatchAsync(int batchSize, TimeSpan lockDuration, CancellationToken cancellationToken = default)
    {
        if (batchSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(batchSize));

        if (lockDuration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(lockDuration));

        var lockToken = Guid.NewGuid();
        var lockedUntilUtc = DateTimeOffset.UtcNow.Add(lockDuration);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var deliveries = await dbContext.EmailDeliveries
            .FromSqlInterpolated($"""
            SELECT *
            FROM email_deliveries
            WHERE status = {(int)EmailDeliveryStatus.Pending}
              AND next_attempt_at_utc <= NOW()
              AND (
                  locked_until_utc IS NULL
                  OR locked_until_utc <= NOW()
              )
            ORDER BY next_attempt_at_utc, created_at_utc
            LIMIT {batchSize}
            FOR UPDATE SKIP LOCKED
            """)
            .ToListAsync(cancellationToken);

        foreach (var delivery in deliveries)
        {
            delivery.AcquireLease(lockToken, lockedUntilUtc);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return deliveries;
    }

    public async Task MarkAsFailedAsync(Guid emailDeliveryId, Guid lockToken, string error, TimeSpan retryDelay,
        int maxAttempts, CancellationToken cancellationToken = default)
    {
        if (retryDelay < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(retryDelay));

        var delivery = await dbContext.EmailDeliveries.SingleOrDefaultAsync(x => x.Id == emailDeliveryId
                     && x.Status == EmailDeliveryStatus.Pending && x.LockToken == lockToken
                     && x.LockedUntilUtc > DateTimeOffset.UtcNow, cancellationToken);

        if (delivery is null)
        {
            throw new InvalidOperationException("Email delivery lease is no longer valid.");
        }

        delivery.MarkFailed(error, DateTimeOffset.UtcNow.Add(retryDelay), maxAttempts);

        delivery.ReleaseLease();

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAsSentAsync(Guid emailDeliveryId, Guid lockToken, DateTimeOffset sentAtUtc, CancellationToken cancellationToken = default)
    {
        var delivery = await dbContext.EmailDeliveries.SingleOrDefaultAsync(x => x.Id == emailDeliveryId
                                                                                 && x.Status == EmailDeliveryStatus.Pending
                                                                                 && x.LockToken == lockToken
                                                                                 && x.LockedUntilUtc > DateTimeOffset.UtcNow, cancellationToken);

        if (delivery is null)
        {
            throw new InvalidOperationException("Email delivery lease is no longer valid.");
        }

        delivery.MarkSent(sentAtUtc);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}