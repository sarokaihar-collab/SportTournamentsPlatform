namespace SportsTournament.Application.Tournaments.Queries.TournamentDetails;

public sealed class TournamentDetailsQuery(ITournamentRepository tournamentRepository) : ITournamentDetailsQuery
{
    public async Task<TournamentListItem?> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var tournament = await tournamentRepository.GetByIdAsync(id, cancellationToken);

        if (tournament is null)
        {
            return null;
        }

        return new TournamentListItem(
            tournament.Id,
            tournament.Name,
            tournament.StartDate,
            tournament.EndDate,
            tournament.MaxTeams,
            tournament.PlayersOnFieldPerTeam,
            tournament.Status.ToString());
    }
}