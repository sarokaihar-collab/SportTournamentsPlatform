namespace SportTournaments.Application.Common.Outbox;

public sealed class OutboxLeaseLostException(Guid messageId) : InvalidOperationException(
        $"The processing lease for outbox message '{messageId}' was lost.");