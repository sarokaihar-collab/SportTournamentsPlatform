namespace SportsTournament.Application.Common.Outbox;

public interface IOutboxProcessor
{
    Task<int> ProcessBatchAsync(CancellationToken cancellationToken = default);
}