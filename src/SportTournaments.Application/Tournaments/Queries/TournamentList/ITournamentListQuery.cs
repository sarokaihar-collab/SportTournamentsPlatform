namespace SportTournaments.Application.Tournaments.Queries;

public interface ITournamentListQuery
{
    Task<IReadOnlyList<TournamentListItem>> ExecuteAsync(CancellationToken cancellationToken);
}