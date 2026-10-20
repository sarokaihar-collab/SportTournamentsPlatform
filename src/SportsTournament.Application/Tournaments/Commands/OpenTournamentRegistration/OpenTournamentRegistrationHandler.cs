using SportsTournament.Application.Tournaments;
using SportsTournament.Application.Tournaments.Commands.OpenTournamentRegistration;

namespace SportsTournament.Application.Tournaments.Commands;

public sealed class OpenTournamentRegistrationHandler(ITournamentRepository tournamentRepository) : IOpenTournamentRegistrationHandler
{
    public async Task<bool> HandleAsync(OpenTournamentRegistrationCommand command, CancellationToken cancellationToken)
    {
        var tournament = await tournamentRepository.GetByIdForUpdateAsync(command.TournamentId, cancellationToken);

        if (tournament is null)
        {
            return false;
        }

        tournament.OpenRegistration(DateTimeOffset.UtcNow);

        await tournamentRepository.UpdateAsync(tournament, cancellationToken);

        return true;
    }
}