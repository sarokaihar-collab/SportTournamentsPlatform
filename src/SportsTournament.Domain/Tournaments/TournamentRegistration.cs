using SportsTournament.Domain.Enums;

namespace SportsTournament.Domain;

public sealed class TournamentRegistration
{
    // Constructor used by EF Core.
    private TournamentRegistration()
    {
    }

    public TournamentRegistration(Guid tournamentId, Guid teamId, RegistrationStatus initialStatus)
    {
        if (tournamentId == Guid.Empty)
        {
            throw new ArgumentException("Tournament ID cannot be empty.", nameof(tournamentId));
        }

        if (teamId == Guid.Empty)
        {
            throw new ArgumentException("Team ID cannot be empty.", nameof(teamId));
        }

        if (initialStatus is not (RegistrationStatus.Confirmed or RegistrationStatus.Waitlisted))
        {
            throw new ArgumentOutOfRangeException(nameof(initialStatus), "A new registration must be Confirmed or Waitlisted.");
        }

        Id = Guid.NewGuid();
        TournamentId = tournamentId;
        TeamId = teamId;
        Status = initialStatus;
        RegisteredAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid TournamentId { get; private set; }

    public Guid TeamId { get; private set; }

    public RegistrationStatus Status { get; private set; }

    public DateTimeOffset RegisteredAtUtc { get; private set; }

    public void ConfirmFromWaitlist()
    {
        if (Status != RegistrationStatus.Waitlisted)
        {
            throw new InvalidOperationException("Only a waitlisted registration can be confirmed.");
        }

        Status = RegistrationStatus.Confirmed;
    }

    public void Cancel()
    {
        if (Status == RegistrationStatus.Cancelled)
        {
            throw new InvalidOperationException("Registration is already cancelled.");
        }

        Status = RegistrationStatus.Cancelled;
    }

    public void ReRegister(RegistrationStatus newStatus, DateTimeOffset registeredAtUtc)
    {
        if (Status != RegistrationStatus.Cancelled)
        {
            throw new InvalidOperationException("Only a cancelled registration can be reactivated.");
        }

        if (newStatus != RegistrationStatus.Confirmed && newStatus != RegistrationStatus.Waitlisted)
        {
            throw new ArgumentOutOfRangeException(nameof(newStatus), "A re-registration must be Confirmed or Waitlisted.");
        }

        if (registeredAtUtc == default)
        {
            throw new ArgumentException("Registration time must be provided.", nameof(registeredAtUtc));
        }

        Status = newStatus;
        RegisteredAtUtc = registeredAtUtc.ToUniversalTime();
    }
}