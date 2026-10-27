using MassTransit;
using SportTournaments.Contracts.Events.Tournaments;

namespace SportTournaments.NotificationService.Consumers;

public sealed class TournamentCreatedConsumer(ILogger<TournamentCreatedConsumer> logger) : IConsumer<TournamentCreatedV1>
{
    public Task Consume(ConsumeContext<TournamentCreatedV1> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Received tournament-created event. " +
            "EventId: {EventId}, TournamentId: {TournamentId}, " +
            "OrganizerId: {OrganizerId}, TournamentName: {TournamentName}",
            message.EventId,
            message.TournamentId,
            message.OrganizerId,
            message.TournamentName);

        return Task.CompletedTask;
    }
}