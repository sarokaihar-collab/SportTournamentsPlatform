using Microsoft.EntityFrameworkCore;
using SportsTournament.Application.Common.Outbox;

namespace SportsTournament.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessageStore(PlatformDbContext dbContext) : IOutboxMessageStore
{
    private const int MaxStoredErrorLength = 2000;

    public async Task<IReadOnlyList<ClaimedOutboxMessage>> ClaimBatchAsync(IReadOnlyCollection<string> messageTypes,
        int batchSize, TimeSpan lockDuration, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(messageTypes);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);

        if (lockDuration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(lockDuration));
        }

        if (messageTypes.Count == 0 || messageTypes.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("At least one valid message type must be provided.", nameof(messageTypes));
        }

        var supportedTypes = messageTypes.Distinct(StringComparer.Ordinal).ToArray();

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var messages = await dbContext.OutboxMessages
            .FromSql($"""
            SELECT *
            FROM public.outbox_messages
            WHERE processed_at_utc IS NULL
              AND next_attempt_at_utc <= statement_timestamp()
              AND (
                  locked_until_utc IS NULL
                  OR locked_until_utc <= statement_timestamp()
              )
              AND message_type = ANY({supportedTypes})
            ORDER BY created_at_utc, id
            LIMIT {batchSize}
            FOR UPDATE SKIP LOCKED
            """)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            await transaction.CommitAsync(cancellationToken);

            return Array.Empty<ClaimedOutboxMessage>();
        }

        // Read the database clock instead of relying on the application
        // server's clock when calculating the lease expiration.
        var databaseNow = await dbContext.Database.SqlQuery<DateTimeOffset>( $"SELECT clock_timestamp() AS \"Value\"")
            .SingleAsync(cancellationToken);

        var lockedUntilUtc = databaseNow.Add(lockDuration);

        foreach (var message in messages)
        {
            message.AcquireLease(Guid.NewGuid(), lockedUntilUtc);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var claimedMessages = messages.Select(message => new ClaimedOutboxMessage(message.Id, message.MessageType, message.Payload, message.AttemptCount,
                message.LockToken ?? throw new InvalidOperationException("The claimed outbox message has no lock token."))).ToArray();

        await transaction.CommitAsync(cancellationToken);

        // Prevent stale tracked entities from affecting later operations
        // if this DbContext is reused.
        foreach (var message in messages)
        {
            dbContext.Entry(message).State = EntityState.Detached;
        }

        return claimedMessages;
    }

    public async Task MarkProcessedAsync(Guid messageId, Guid lockToken, CancellationToken cancellationToken = default)
    {
        var updatedCount = await dbContext.Database
            .ExecuteSqlInterpolatedAsync($"""
            UPDATE public.outbox_messages
            SET processed_at_utc = clock_timestamp(),
                locked_until_utc = NULL,
                lock_token = NULL,
                last_error = NULL
            WHERE id = {messageId}
              AND lock_token = {lockToken}
              AND processed_at_utc IS NULL
              AND locked_until_utc > clock_timestamp();
            """,
                cancellationToken);

        if (updatedCount == 0)
        {
            throw new OutboxLeaseLostException(messageId);
        }
    }

    public async Task ScheduleRetryAsync(Guid messageId, Guid lockToken, TimeSpan retryDelay, string? lastError,
        CancellationToken cancellationToken = default)
    {
        if (retryDelay <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(retryDelay));
        }

        var storedError = lastError is { Length: > MaxStoredErrorLength } ? lastError[..MaxStoredErrorLength] : lastError;

        var updatedCount = await dbContext.Database
            .ExecuteSqlInterpolatedAsync($"""
            UPDATE public.outbox_messages
            SET next_attempt_at_utc = clock_timestamp() + {retryDelay},
                last_error = {storedError},
                locked_until_utc = NULL,
                lock_token = NULL
            WHERE id = {messageId}
              AND lock_token = {lockToken}
              AND processed_at_utc IS NULL
              AND locked_until_utc > clock_timestamp();
            """,
                cancellationToken);

        if (updatedCount == 0)
        {
            throw new OutboxLeaseLostException(messageId);
        }
    }
}