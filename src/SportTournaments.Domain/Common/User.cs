using SportTournaments.Domain.Exceptions;
using System.Net.Mail;

namespace SportTournaments.Domain.Common;

public sealed class User
{
    private const int MaxDisplayNameLength = 100;
    private const int MaxEmailLength = 254;

    // Constructor used by EF Core.
    private User()
    {
    }

    public User(string displayName, string email)
    {
        Id = Guid.NewGuid();
        DisplayName = ValidateDisplayName(displayName);
        Email = ValidateEmail(email);
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public string DisplayName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public void Rename(string displayName)
    {
        DisplayName = ValidateDisplayName(displayName);
    }

    public void ChangeEmail(string email)
    {
        Email = ValidateEmail(email);
    }

    private static string ValidateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new DomainValidationException("Display name cannot be empty.", nameof(displayName));
        }

        var normalizedName = displayName.Trim();

        if (normalizedName.Length > MaxDisplayNameLength)
        {
            throw new DomainValidationException($"Display name cannot exceed {MaxDisplayNameLength} characters.", nameof(displayName));
        }

        return normalizedName;
    }

    private static string ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainValidationException("Email cannot be empty.", nameof(email));
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();

        if (normalizedEmail.Length > MaxEmailLength)
        {
            throw new DomainValidationException($"Email cannot exceed {MaxEmailLength} characters.", nameof(email));
        }

        if (!MailAddress.TryCreate(normalizedEmail, out var mailAddress)
            || !string.Equals(mailAddress.Address, normalizedEmail, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainValidationException("Email address is invalid.", nameof(email));
        }

        return normalizedEmail;
    }
}