namespace SportsTournament.Application.Common.Outbox;

public interface IOutboxMessageWriter
{
    void Enqueue<T>(string messageType, T payload)
        where T : class;
}