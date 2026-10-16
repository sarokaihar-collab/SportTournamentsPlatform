using SportsTournament.Domain.Enums;

namespace SportsTournament.Domain.Tournaments;

public sealed class Tournament
{
    private const int MinTeams = 3;
    private const int MaxNameLength = 100;

    // Constructor used by EF Core.
    private Tournament()
    {
    }

    public Tournament(string name, Guid organizerId, DateTimeOffset registrationDeadlineUtc, DateOnly startDate, DateOnly endDate, int maxTeams)
    {
        Id = Guid.NewGuid();
        Name = ValidateName(name);

        if (organizerId == Guid.Empty)
        {
            throw new ArgumentException("Organizer ID cannot be empty.", nameof(organizerId));
        }

        if (registrationDeadlineUtc == default)
        {
            throw new ArgumentException("Registration deadline must be specified.", nameof(registrationDeadlineUtc));
        }

        if (startDate == default || endDate == default)
        {
            throw new ArgumentException("Tournament dates must be specified.");
        }

        if (startDate > endDate)
        {
            throw new ArgumentException("Tournament start date cannot be after end date.");
        }

        ValidateMaxTeams(maxTeams);

        OrganizerId = organizerId;
        RegistrationDeadlineUtc = registrationDeadlineUtc.ToUniversalTime();

        StartDate = startDate;
        EndDate = endDate;
        MaxTeams = maxTeams;

        Status = TournamentStatus.Draft;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public Guid OrganizerId { get; private set; }

    public DateTimeOffset RegistrationDeadlineUtc { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly EndDate { get; private set; }

    public int MaxTeams { get; private set; }

    public TournamentStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public void Rename(string name)
    {
        EnsureNotCancelled();
        Name = ValidateName(name);
    }

    public void ChangeMaxTeams(int newMaxTeams, int confirmedTeamsCount)
    {
        if (Status != TournamentStatus.RegistrationOpen)
        {
            throw new InvalidOperationException("Capacity can only be changed while registration is open.");
        }

        if (confirmedTeamsCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(confirmedTeamsCount));
        }

        ValidateMaxTeams(newMaxTeams);

        if (newMaxTeams < confirmedTeamsCount)
        {
            throw new InvalidOperationException("Capacity cannot be lower than the number of confirmed teams.");
        }

        MaxTeams = newMaxTeams;
    }

    public void OpenRegistration(DateTimeOffset now)
    {
        EnsureStatus(TournamentStatus.Draft);

        if (RegistrationDeadlineUtc <= now.ToUniversalTime())
        {
            throw new InvalidOperationException("Registration deadline must be in the future.");
        }

        Status = TournamentStatus.RegistrationOpen;
    }

    public void CloseRegistration()
    {
        EnsureStatus(TournamentStatus.RegistrationOpen);

        Status = TournamentStatus.RegistrationClosed;
    }

    public void Start()
    {
        EnsureStatus(TournamentStatus.RegistrationClosed);

        Status = TournamentStatus.InProgress;
    }

    public void Complete()
    {
        EnsureStatus(TournamentStatus.InProgress);

        Status = TournamentStatus.Completed;
    }

    public void Cancel()
    {
        if (Status is TournamentStatus.InProgress or TournamentStatus.Completed or TournamentStatus.Cancelled)
        {
            throw new InvalidOperationException("Tournament cannot be cancelled in its current state.");
        }

        Status = TournamentStatus.Cancelled;
    }

    private void EnsureStatus(TournamentStatus expectedStatus)
    {
        if (Status != expectedStatus)
        {
            throw new InvalidOperationException($"Operation is not allowed when tournament status is {Status}.");
        }
    }

    private void EnsureNotCancelled()
    {
        if (Status == TournamentStatus.Cancelled)
        {
            throw new InvalidOperationException("Cancelled tournament cannot be modified.");
        }
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Tournament name cannot be empty.", nameof(name));
        }

        var normalizedName = name.Trim();

        if (normalizedName.Length > MaxNameLength)
        {
            throw new ArgumentOutOfRangeException(nameof(name), $"Tournament name cannot exceed {MaxNameLength} characters.");
        }

        return normalizedName;
    }

    private static void ValidateMaxTeams(int maxTeams)
    {
        if (maxTeams < MinTeams)
        {
            throw new ArgumentOutOfRangeException(nameof(maxTeams), $"Tournament must allow at least {MinTeams} teams.");
        }
    }
}