public interface IOutboxMessageDispatcher
{
    Task DispatchAsync(string messageType, string payload, CancellationToken cancellationToken = default);
}