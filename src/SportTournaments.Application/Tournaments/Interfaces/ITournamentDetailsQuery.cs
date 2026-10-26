using SportTournaments.Application.Tournaments.Queries;

namespace SportTournaments.Application.Tournaments.Interfaces;

public interface ITournamentDetailsQuery
{
    Task<TournamentListItem?> ExecuteAsync(Guid id, CancellationToken cancellationToken);
}