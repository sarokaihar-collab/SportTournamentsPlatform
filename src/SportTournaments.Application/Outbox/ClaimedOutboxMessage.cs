namespace SportTournaments.Application.Common.Outbox;

public sealed record ClaimedOutboxMessage(
    Guid Id,
    string MessageType,
    string Payload,
    int AttemptCount,
    Guid LockToken);