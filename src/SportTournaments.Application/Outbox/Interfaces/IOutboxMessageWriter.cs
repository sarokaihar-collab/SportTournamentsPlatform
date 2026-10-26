namespace SportTournaments.Application.Common.Outbox.Interfaces;

public interface IOutboxMessageWriter
{
    void Enqueue<T>(string messageType, T payload) where T : class;
}