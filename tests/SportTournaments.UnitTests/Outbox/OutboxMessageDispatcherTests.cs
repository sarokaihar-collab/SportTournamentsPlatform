using SportTournaments.Application.Common.Outbox;
using SportTournaments.Application.Tournaments;
using SportTournaments.Contracts.Events.Tournaments;
using SportTournaments.UnitTests.Common.Fakes;
using System.Text.Json;

public class OutboxMessageDispatcherTests
{
    private readonly FakeCacheService _cacheService = new();

    private TournamentCacheInvalidationHandler CreateSut() => new(_cacheService);

    [Fact]
    public async Task DispatchAsync_TournamentCreated_PublishesIntegrationEvent()
    {
        // Arrange
        var publisher = new FakeIntegrationEventPublisher();

        var cacheHandler = CreateSut();

        var dispatcher = new OutboxMessageDispatcher(cacheHandler, publisher);

        var message = new TournamentCreatedV1(
            EventId: Guid.NewGuid(),
            TournamentId: Guid.NewGuid(),
            OrganizerId: Guid.NewGuid(),
            TournamentName: "Test tournament",
            OccurredAtUtc: DateTimeOffset.UtcNow);

        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        var payload = JsonSerializer.Serialize(message, jsonOptions);

        // Act
        await dispatcher.DispatchAsync(OutboxMessageTypes.TournamentCreated, payload, CancellationToken.None);

        // Assert
        var published = Assert.Single(publisher.PublishedEvents);

        var actual = Assert.IsType<TournamentCreatedV1>(published);

        Assert.Equal(message.EventId, actual.EventId);
        Assert.Equal(message.TournamentId, actual.TournamentId);
        Assert.Equal(message.OrganizerId, actual.OrganizerId);
        Assert.Equal(message.TournamentName, actual.TournamentName);
        Assert.Equal(message.OccurredAtUtc, actual.OccurredAtUtc);
    }

    private sealed class FakeIntegrationEventPublisher : IIntegrationEventPublisher
    {
        public List<object> PublishedEvents { get; } = [];

        public Task PublishAsync<TEvent>(TEvent integrationEvent,  CancellationToken cancellationToken = default)
            where TEvent : class
        {
            PublishedEvents.Add(integrationEvent);
            return Task.CompletedTask;
        }
    }
}