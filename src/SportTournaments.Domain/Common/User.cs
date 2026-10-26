using SportTournaments.Domain.Exceptions;

namespace SportTournaments.Domain.Common;

public sealed class User
{
    private const int MaxDisplayNameLength = 100;

    // Constructor used by EF Core.
    private User()
    {
    }

    public User(string displayName)
    {
        Id = Guid.NewGuid();
        DisplayName = ValidateDisplayName(displayName);
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public string DisplayName { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public void Rename(string displayName)
    {
        DisplayName = ValidateDisplayName(displayName);
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
}