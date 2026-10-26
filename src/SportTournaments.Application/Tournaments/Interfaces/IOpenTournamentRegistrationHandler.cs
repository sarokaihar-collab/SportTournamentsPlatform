using SportTournaments.Application.Tournaments.Commands.OpenTournamentRegistration;

namespace SportTournaments.Application.Tournaments.Interfaces;

public interface IOpenTournamentRegistrationHandler
{
    Task<bool> HandleAsync(OpenTournamentRegistrationCommand command, CancellationToken cancellationToken);
}