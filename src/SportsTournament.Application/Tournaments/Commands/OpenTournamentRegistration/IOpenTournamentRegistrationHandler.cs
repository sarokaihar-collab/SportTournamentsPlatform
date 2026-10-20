namespace SportsTournament.Application.Tournaments.Commands.OpenTournamentRegistration;

public interface IOpenTournamentRegistrationHandler
{
    Task<bool> HandleAsync(OpenTournamentRegistrationCommand command, CancellationToken cancellationToken);
}