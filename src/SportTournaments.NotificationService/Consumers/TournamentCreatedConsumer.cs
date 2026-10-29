using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SportTournaments.Contracts.Events.Tournaments;
using SportTournaments.NotificationService.Persistence;
using SportTournaments.NotificationService.Persistence.Entities;

namespace SportTournaments.NotificationService.Consumers;

public sealed class TournamentCreatedConsumer(NotificationDbContext dbContext, ILogger<TournamentCreatedConsumer> logger)
    : IConsumer<TournamentCreatedV1>
{
    private const string NotificationEventIdUniqueIndex = "ux_notifications_event_id";

    private const string EmailDeliveryEventRecipientUniqueIndex = "ux_email_deliveries_event_recipient";

    public async Task Consume(ConsumeContext<TournamentCreatedV1> context)
    {
        var message = context.Message;

        if (string.IsNullOrWhiteSpace(message.OrganizerEmail))
        {
            logger.LogError("Organizer email is missing for event {EventId}.", message.EventId);

            throw new InvalidOperationException($"Organizer email is missing for event {message.EventId}.");
        }

        const string title = "Турнир создан";
        var body = $"Турнир «{message.TournamentName}» создан.";

        var notification = new Notification(
            eventId: message.EventId,
            recipientUserId: message.OrganizerId,
            tournamentId: message.TournamentId,
            notificationType: "TournamentCreated",
            title: title,
            message: body,
            eventOccurredAtUtc: message.OccurredAtUtc);

        var emailDelivery = EmailDelivery.Create(
            eventId: message.EventId,
            recipientUserId: message.OrganizerId,
            recipientEmail: message.OrganizerEmail,
            subject: title,
            body: body);

        dbContext.Notifications.Add(notification);
        dbContext.EmailDeliveries.Add(emailDelivery);

        try
        {
            // Both records are saved through the same DbContext.
            await dbContext.SaveChangesAsync(context.CancellationToken);
        }
        catch (DbUpdateException exception) when (IsDuplicateEvent(exception))
        {
            dbContext.Entry(notification).State = EntityState.Detached;
            dbContext.Entry(emailDelivery).State = EntityState.Detached;

            logger.LogInformation("Notification and email delivery for event {EventId} " + "already exist. Duplicate delivery was ignored.",
                message.EventId);

            return;
        }

        logger.LogInformation(
            "Notification {NotificationId} and email delivery " + "{EmailDeliveryId} saved for event {EventId}.",
            notification.Id, emailDelivery.Id, message.EventId);
    }

    private static bool IsDuplicateEvent(DbUpdateException exception)
    {
        if (exception.InnerException is not PostgresException postgresException
            || postgresException.SqlState != PostgresErrorCodes.UniqueViolation)
        {
            return false;
        }

        return postgresException.ConstraintName is NotificationEventIdUniqueIndex or EmailDeliveryEventRecipientUniqueIndex;
    }
}