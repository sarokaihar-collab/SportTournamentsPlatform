using SportTournaments.Domain.Exceptions;

public sealed class UserIdentity
{
    private const int MaxIssuerLength = 500;
    private const int MaxSubjectLength = 255;

    // Constructor used by EF Core.
    private UserIdentity()
    {
    }

    public UserIdentity(Guid userId, string issuer, string subject)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainValidationException("User ID cannot be empty.", nameof(userId));
        }

        Id = Guid.NewGuid();
        UserId = userId;
        Issuer = ValidateIssuer(issuer);
        Subject = ValidateSubject(subject);
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string Issuer { get; private set; } = string.Empty;

    public string Subject { get; private set; } = string.Empty;

    private static string ValidateIssuer(string issuer)
    {
        if (string.IsNullOrWhiteSpace(issuer))
        {
            throw new DomainValidationException("Issuer cannot be empty.", nameof(issuer));
        }

        if (issuer.Length > MaxIssuerLength)
        {
            throw new DomainValidationException($"Issuer cannot exceed {MaxIssuerLength} characters.", nameof(issuer));
        }

        return issuer;
    }

    private static string ValidateSubject(string subject)
    {
        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new DomainValidationException("Subject cannot be empty.", nameof(subject));
        }

        if (subject.Length > MaxSubjectLength)
        {
            throw new DomainValidationException($"Subject cannot exceed {MaxSubjectLength} characters.", nameof(subject));
        }

        return subject;
    }
}