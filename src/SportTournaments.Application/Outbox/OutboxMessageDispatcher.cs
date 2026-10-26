using SportTournaments.Application.Outbox.Messages;
using SportTournaments.Application.Tournaments;
using System.Text.Json;

namespace SportTournaments.Application.Common.Outbox;

public sealed class OutboxMessageDispatcher(TournamentCacheInvalidationHandler cacheInvalidationHandler) : IOutboxMessageDispatcher
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task DispatchAsync(string messageType, string payload, CancellationToken cancellationToken = default)
    {
        switch (messageType)
        {
            case OutboxMessageTypes.TournamentListCacheInvalidationRequested:
                {
                    var message = DeserializeRequired<TournamentListCacheInvalidationRequested>(payload);

                    await cacheInvalidationHandler.HandleAsync(message, cancellationToken);

                    break;
                }

            case OutboxMessageTypes.TournamentDetailsCacheInvalidationRequested:
                {
                    var message = DeserializeRequired<TournamentDetailsCacheInvalidationRequested>(payload);

                    await cacheInvalidationHandler.HandleAsync(message, cancellationToken);

                    break;
                }

            default:
                throw new NotSupportedException($"Outbox message type '{messageType}' is not supported.");
        }
    }

    private static T DeserializeRequired<T>(string payload) where T : class
    {
        return JsonSerializer.Deserialize<T>(payload, SerializerOptions)
            ?? throw new InvalidOperationException("Outbox message payload deserialized to null.");
    }
}