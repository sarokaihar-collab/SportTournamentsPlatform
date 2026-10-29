namespace SportTournaments.NotificationService.Persistence.Entities;

public sealed class Notification
{
    private Notification()
    {
    }

    public Notification(Guid eventId, Guid recipientUserId, Guid? tournamentId, string notificationType,
        string title, string message, DateTimeOffset eventOccurredAtUtc)
    {
        if (eventId == Guid.Empty)
            throw new ArgumentException("EventId cannot be empty.", nameof(eventId));

        if (recipientUserId == Guid.Empty)
            throw new ArgumentException("RecipientUserId cannot be empty.", nameof(recipientUserId));

        ArgumentException.ThrowIfNullOrWhiteSpace(notificationType);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);


        Id = Guid.NewGuid();
        EventId = eventId;
        RecipientUserId = recipientUserId;
        TournamentId = tournamentId;
        NotificationType = notificationType;
        Title = title;
        Message = message;
        EventOccurredAtUtc = eventOccurredAtUtc;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid EventId { get; private set; }

    public Guid RecipientUserId { get; private set; }

    public Guid? TournamentId { get; private set; }

    public string NotificationType { get; private set; } = null!;

    public string Title { get; private set; } = null!;

    public string Message { get; private set; } = null!;

    public DateTimeOffset EventOccurredAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? ReadAtUtc { get; private set; }
}