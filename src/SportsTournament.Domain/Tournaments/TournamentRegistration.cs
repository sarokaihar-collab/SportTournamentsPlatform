using SportsTournament.Domain.Enums;
using SportsTournament.Domain.Exceptions;

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
            throw new DomainValidationException("Tournament ID cannot be empty.", nameof(tournamentId));
        }

        if (teamId == Guid.Empty)
        {
            throw new DomainValidationException("Team ID cannot be empty.", nameof(teamId));
        }

        if (initialStatus is not (RegistrationStatus.Confirmed or RegistrationStatus.Waitlisted))
        {
            throw new DomainValidationException("A new registration must be Confirmed or Waitlisted.", nameof(initialStatus));
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
            throw new DomainOperationNotAllowedException("Only a waitlisted registration can be confirmed.");
        }

        Status = RegistrationStatus.Confirmed;
    }

    public void Cancel()
    {
        if (Status == RegistrationStatus.Cancelled)
        {
            throw new DomainOperationNotAllowedException("Registration is already cancelled.");
        }

        Status = RegistrationStatus.Cancelled;
    }

    public void ReRegister(RegistrationStatus newStatus, DateTimeOffset registeredAtUtc)
    {
        if (Status != RegistrationStatus.Cancelled)
        {
            throw new DomainOperationNotAllowedException("Only a cancelled registration can be reactivated.");
        }

        if (newStatus is not (RegistrationStatus.Confirmed or RegistrationStatus.Waitlisted))
        {
            throw new DomainValidationException("A re-registration must be Confirmed or Waitlisted.", nameof(newStatus));
        }

        if (registeredAtUtc == default)
        {
            throw new DomainValidationException("Registration time must be provided.", nameof(registeredAtUtc));
        }

        Status = newStatus;
        RegisteredAtUtc = registeredAtUtc.ToUniversalTime();
    }
}