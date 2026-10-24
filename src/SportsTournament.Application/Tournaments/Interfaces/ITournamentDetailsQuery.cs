using SportsTournament.Application.Tournaments.Queries;

namespace SportsTournament.Application.Tournaments.Interfaces;

public interface ITournamentDetailsQuery
{
    Task<TournamentListItem?> ExecuteAsync(Guid id, CancellationToken cancellationToken);
}