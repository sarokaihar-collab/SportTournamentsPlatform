namespace SportTournaments.NotificationService.Persistence.Entities;

public enum EmailDeliveryStatus
{
    Pending = 0,
    Sent = 1,
    DeadLetter = 2
}

public sealed class EmailDelivery
{
    private const int MaxEmailLength = 254;
    private const int MaxSubjectLength = 200;
    private const int MaxErrorLength = 2000;

    private EmailDelivery()
    {
    }

    public Guid Id { get; private set; }

    public Guid EventId { get; private set; }

    public Guid RecipientUserId { get; private set; }

    public string RecipientEmail { get; private set; } = string.Empty;

    public string Subject { get; private set; } = string.Empty;

    public string Body { get; private set; } = string.Empty;

    public EmailDeliveryStatus Status { get; private set; }

    public int AttemptCount { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset NextAttemptAtUtc { get; private set; }

    public DateTimeOffset? SentAtUtc { get; private set; }

    public string? LastError { get; private set; }

    public static EmailDelivery Create(Guid eventId, Guid recipientUserId, string recipientEmail, string subject, string body)
    {
        if (eventId == Guid.Empty)
            throw new ArgumentException("Event ID cannot be empty.", nameof(eventId));

        if (recipientUserId == Guid.Empty)
            throw new ArgumentException("Recipient user ID cannot be empty.", nameof(recipientUserId));

        if (string.IsNullOrWhiteSpace(recipientEmail))
            throw new ArgumentException("Recipient email cannot be empty.", nameof(recipientEmail));

        var normalizedEmail = recipientEmail.Trim();

        if (normalizedEmail.Length > MaxEmailLength
            || !System.Net.Mail.MailAddress.TryCreate(normalizedEmail, out var address)
            || !string.Equals(address.Address, normalizedEmail, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Recipient email is invalid.", nameof(recipientEmail));
        }

        if (string.IsNullOrWhiteSpace(subject) || subject.Length > MaxSubjectLength)
        {
            throw new ArgumentException("Subject is invalid.", nameof(subject));
        }

        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Email body cannot be empty.", nameof(body));

        var now = DateTimeOffset.UtcNow;

        return new EmailDelivery
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            RecipientUserId = recipientUserId,
            RecipientEmail = normalizedEmail,
            Subject = subject,
            Body = body,
            Status = EmailDeliveryStatus.Pending,
            AttemptCount = 0,
            CreatedAtUtc = now,
            NextAttemptAtUtc = now
        };
    }

    public void MarkSent(DateTimeOffset sentAtUtc)
    {
        Status = EmailDeliveryStatus.Sent;
        SentAtUtc = sentAtUtc;
        AttemptCount++;
        LastError = null;
    }

    public void MarkFailed(
        string error,
        DateTimeOffset nextAttemptAtUtc,
        int maxAttempts)
    {
        if (maxAttempts < 1)
            throw new ArgumentOutOfRangeException(nameof(maxAttempts));

        AttemptCount++;

        var normalizedError = string.IsNullOrWhiteSpace(error)
            ? "Email delivery failed."
            : error.Trim();

        LastError = normalizedError.Length <= MaxErrorLength
            ? normalizedError
            : normalizedError[..MaxErrorLength];

        Status = AttemptCount >= maxAttempts
            ? EmailDeliveryStatus.DeadLetter
            : EmailDeliveryStatus.Pending;

        NextAttemptAtUtc = nextAttemptAtUtc;
    }
}