namespace SportsTournament.Application.Tournaments.Queries;

public interface ITournamentDetailsQuery
{
    Task<TournamentListItem?> ExecuteAsync(Guid id, CancellationToken cancellationToken);
}