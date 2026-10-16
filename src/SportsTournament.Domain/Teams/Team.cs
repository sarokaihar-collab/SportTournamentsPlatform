namespace SportsTournament.Domain.Teams;

public sealed class Team
{
    private const int MaxNameLength = 100;

    // Конструктор для EF Core
    private Team()
    {
    }

    public Team(string name)
    {
        Id = Guid.NewGuid();
        Rename(name);
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Team name cannot be empty.", nameof(name));
        }

        var normalizedName = name.Trim();

        if (normalizedName.Length > MaxNameLength)
        {
            throw new ArgumentOutOfRangeException(nameof(name), $"Team name cannot exceed {MaxNameLength} characters.");
        }

        Name = normalizedName;
    }
}