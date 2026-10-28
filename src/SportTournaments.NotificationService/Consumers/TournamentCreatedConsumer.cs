using Microsoft.EntityFrameworkCore;
using Npgsql;
using MassTransit;
using SportTournaments.Contracts.Events.Tournaments;
using SportTournaments.NotificationService.Persistence;
using SportTournaments.NotificationService.Persistence.Entities;

namespace SportTournaments.NotificationService.Consumers;

public sealed class TournamentCreatedConsumer(NotificationDbContext dbContext, ILogger<TournamentCreatedConsumer> logger)
    : IConsumer<TournamentCreatedV1>
{
    private const string EventIdUniqueIndex = "ux_notifications_event_id";

    public async Task Consume(ConsumeContext<TournamentCreatedV1> context)
    {
        var message = context.Message;

        var notification = Notification.Create(
            eventId: message.EventId,
            recipientUserId: message.OrganizerId,
            tournamentId: message.TournamentId,
            notificationType: "TournamentCreated",
            title: "Турнир создан",
            message: $"Турнир «{message.TournamentName}» создан.",
            eventOccurredAtUtc: message.OccurredAtUtc);

        dbContext.Notifications.Add(notification);

        try
        {
            await dbContext.SaveChangesAsync(context.CancellationToken);
        }
        catch (DbUpdateException exception) when (IsDuplicateEventId(exception))
        {
            dbContext.Entry(notification).State = EntityState.Detached;

            logger.LogInformation("Notification for event {EventId} already exists. " + "Duplicate delivery was ignored.", message.EventId);
            
            return;
        }

        logger.LogInformation("Notification {NotificationId} saved for event {EventId} " + "and recipient {RecipientUserId}.",
            notification.Id, message.EventId, message.OrganizerId);
    }

    private static bool IsDuplicateEventId(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException postgresException
            && postgresException.SqlState == PostgresErrorCodes.UniqueViolation
            && postgresException.ConstraintName == EventIdUniqueIndex;
    }
}