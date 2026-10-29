using MassTransit;
using SportTournaments.NotificationService.Consumers;

namespace SportTournaments.NotificationService.Messaging;

public static class RabbitMqServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMq = configuration.GetSection("RabbitMq");

        var hostName = rabbitMq["Host"] ?? "localhost";
        var virtualHost = rabbitMq["VirtualHost"] ?? "/";

        var port = ushort.TryParse(rabbitMq["Port"], out var configuredPort) ? configuredPort : (ushort)5672;

        var username = rabbitMq["Username"];
        var password = rabbitMq["Password"];

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("RabbitMQ credentials must be configured via User Secrets or environment variables.");
        }

        services.AddMassTransit(x =>
        {
            x.AddConsumer<TournamentCreatedConsumer>().Endpoint(e => e.Name = "notification-tournament-created");

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(hostName, port, virtualHost,
                    h =>
                    {
                        h.Username(username);
                        h.Password(password);
                    });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}