using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SportsTournament.Application.Common.Outbox;

public sealed class OutboxProcessor(IOutboxMessageStore messageStore, IOutboxMessageDispatcher dispatcher,
    IOptions<OutboxProcessingSettings> options, ILogger<OutboxProcessor> logger)
{
    private static readonly string[] SupportedMessageTypes =
        [
            OutboxMessageTypes.TournamentListCacheInvalidationRequested,
            OutboxMessageTypes.TournamentDetailsCacheInvalidationRequested
        ];

    public async Task<int> ProcessBatchAsync(CancellationToken cancellationToken = default)
    {
        var settings = options.Value;

        var messages = await messageStore.ClaimBatchAsync(SupportedMessageTypes, settings.BatchSize, settings.LockDuration, cancellationToken);

        foreach (var message in messages)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await dispatcher.DispatchAsync(message.MessageType, message.Payload, cancellationToken);
                await messageStore.MarkProcessedAsync(message.Id, message.LockToken, cancellationToken);

                logger.LogDebug("Outbox message {OutboxMessageId} of type {MessageType} processed.", message.Id, message.MessageType);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Let the worker stop. The lease will expire if the
                // message was claimed but not marked as processed.
                throw;
            }
            catch (OutboxLeaseLostException exception)
            {
                logger.LogWarning(exception, "Lease lost for outbox message {OutboxMessageId}. Another worker may own it.", message.Id);
            }
            catch (Exception exception)
            {
                var retryDelay = CalculateRetryDelay(message.AttemptCount, settings.InitialRetryDelay, settings.MaxRetryDelay);

                var nextAttemptAtUtc = DateTimeOffset.UtcNow.Add(retryDelay);

                // Persist only the exception type, not an arbitrary exception
                // message that might contain sensitive implementation details.
                var safeError = exception.GetType().Name;

                try
                {
                    await messageStore.ScheduleRetryAsync(message.Id, message.LockToken, nextAttemptAtUtc, safeError, cancellationToken);
                }
                catch (OutboxLeaseLostException leaseException)
                {
                    logger.LogWarning(leaseException,
                        "Could not schedule retry for outbox message {OutboxMessageId} because its lease was lost.",
                        message.Id);

                    continue;
                }

                logger.LogWarning(
                    exception,
                    "Outbox message {OutboxMessageId} failed. A retry is scheduled after {RetryDelay}. Error type: {ErrorType}.",
                    message.Id,
                    retryDelay,
                    safeError);
            }
        }

        // Return the number claimed, not the number successfully processed.
        return messages.Count;
    }

    private static TimeSpan CalculateRetryDelay(int attemptCount, TimeSpan initialDelay, TimeSpan maxDelay)
    {
        var exponent = Math.Clamp(attemptCount - 1, 0, 30);

        var delayMilliseconds = Math.Min(initialDelay.TotalMilliseconds * Math.Pow(2, exponent), maxDelay.TotalMilliseconds);

        return TimeSpan.FromMilliseconds(delayMilliseconds);
    }
}