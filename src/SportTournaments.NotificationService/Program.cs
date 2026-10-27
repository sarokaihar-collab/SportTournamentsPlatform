using MassTransit;
using Microsoft.EntityFrameworkCore;
using SportTournaments.NotificationService.Consumers;
using SportTournaments.NotificationService.Persistence;

var builder = Host.CreateApplicationBuilder(args);

var rabbitMq = builder.Configuration.GetSection("RabbitMq");

var hostName = rabbitMq["Host"] ?? "localhost";
var virtualHost = rabbitMq["VirtualHost"] ?? "/";

var port = ushort.TryParse(rabbitMq["Port"], out var configuredPort) ? configuredPort : (ushort)5672;

var username = rabbitMq["Username"];
var password = rabbitMq["Password"];

if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
{
    throw new InvalidOperationException("RabbitMQ credentials must be configured via User Secrets or environment variables.");
}

builder.Services.AddMassTransit(x =>
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

var connectionString = builder.Configuration.GetConnectionString("NotificationDatabase");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'NotificationDatabase' is not configured.");
}

builder.Services.AddDbContext<NotificationDbContext>(options => options.UseNpgsql(connectionString));

var host = builder.Build();

host.Run();