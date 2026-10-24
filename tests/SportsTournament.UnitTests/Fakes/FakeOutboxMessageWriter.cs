using SportsTournament.Application.Common.Outbox.Interfaces;

namespace SportsTournament.UnitTests.Common.Fakes;

public sealed class FakeOutboxMessageWriter : IOutboxMessageWriter
{
    public List<EnqueuedOutboxMessage> Messages { get; } = [];

    public void Enqueue<T>(string messageType, T payload) where T : class
    {
        Messages.Add(new EnqueuedOutboxMessage(messageType, payload));
    }
}

public sealed record EnqueuedOutboxMessage(string MessageType, object Payload);