namespace SportTournaments.Application.Common.Outbox;

public interface IOutboxProcessor
{
    Task<int> ProcessBatchAsync(IReadOnlyCollection<string> messageTypes, CancellationToken cancellationToken = default);
}