using SportsTournament.Application.Tournaments.Commands.OpenTournamentRegistration;

namespace SportsTournament.Application.Tournaments.Interfaces;

public interface IOpenTournamentRegistrationHandler
{
    Task<bool> HandleAsync(OpenTournamentRegistrationCommand command, CancellationToken cancellationToken);
}