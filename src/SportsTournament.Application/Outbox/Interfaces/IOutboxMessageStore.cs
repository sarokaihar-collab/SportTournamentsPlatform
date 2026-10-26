namespace SportsTournament.Application.Common.Outbox;

public interface IOutboxMessageStore
{
    Task<IReadOnlyList<ClaimedOutboxMessage>> ClaimBatchAsync(IReadOnlyCollection<string> messageTypes, int batchSize, TimeSpan lockDuration,
        CancellationToken cancellationToken = default);
    Task MarkProcessedAsync(Guid messageId, Guid lockToken, CancellationToken cancellationToken = default);

    Task ScheduleRetryAsync(Guid messageId, Guid lockToken, TimeSpan retryDelay, string? lastError,
        CancellationToken cancellationToken = default);
}