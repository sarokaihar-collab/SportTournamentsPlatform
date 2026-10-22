namespace SportsTournament.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessage
{
    private OutboxMessage()
    {
    }

    public OutboxMessage(
        string messageType,
        string payload,
        DateTimeOffset createdAtUtc)
    {
        Id = Guid.NewGuid();
        MessageType = messageType;
        Payload = payload;
        CreatedAtUtc = createdAtUtc;
        NextAttemptAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public string MessageType { get; private set; } = null!;

    public string Payload { get; private set; } = null!;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? ProcessedAtUtc { get; private set; }

    public int AttemptCount { get; private set; }

    public DateTimeOffset NextAttemptAtUtc { get; private set; }

    public DateTimeOffset? LockedUntilUtc { get; private set; }

    public Guid? LockToken { get; private set; }

    public string? LastError { get; private set; }
}