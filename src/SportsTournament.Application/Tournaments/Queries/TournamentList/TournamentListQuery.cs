using SportsTournament.Application.Tournaments;
using SportsTournament.Application.Tournaments.Queries;

public sealed class TournamentListQuery(ITournamentRepository tournamentRepository) : ITournamentListQuery
{
    public async Task<IReadOnlyList<TournamentListItem>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var tournaments = await tournamentRepository.GetAllAsync(cancellationToken);

        return tournaments
            .Select(tournament => new TournamentListItem(
                tournament.Id,
                tournament.Name,
                tournament.StartDate,
                tournament.EndDate,
                tournament.MaxTeams,
                tournament.PlayersOnFieldPerTeam,
                tournament.Status.ToString()))
            .ToList();
    }
}