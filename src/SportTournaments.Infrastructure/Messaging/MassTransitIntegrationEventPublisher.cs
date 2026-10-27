using MassTransit;
using SportTournaments.Application.Common.Outbox;

namespace SportTournaments.Infrastructure.Messaging;

public sealed class MassTransitIntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitIntegrationEventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)  where TEvent : class
    {
        return _publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}