using System.Text.Json;
using SportTournaments.Application.Common.Outbox.Interfaces;

namespace SportTournaments.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessageWriter(PlatformDbContext dbContext) : IOutboxMessageWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Enqueue<T>(string messageType, T payload) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageType);
        ArgumentNullException.ThrowIfNull(payload);

        var json = JsonSerializer.Serialize(payload, JsonOptions);

        var message = new OutboxMessage(messageType, json, DateTimeOffset.UtcNow);

        dbContext.OutboxMessages.Add(message);
    }
}