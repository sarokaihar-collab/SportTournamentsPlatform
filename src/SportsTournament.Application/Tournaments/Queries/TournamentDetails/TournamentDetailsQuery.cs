using SportsTournament.Application.Common;
using SportsTournament.Application.Tournaments;
using SportsTournament.Application.Tournaments.Queries;

public sealed class TournamentDetailsQuery(ITournamentRepository tournamentRepository, ICacheService cacheService) 
    : ITournamentDetailsQuery
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public async Task<TournamentListItem?> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var cacheKey = TournamentCacheKeys.Details(id);

        var cached = await cacheService.GetAsync<TournamentListItem>(cacheKey, cancellationToken);

        if (cached is not null)
        {
            return cached;
        }

        var tournament = await tournamentRepository.GetByIdAsync(id, cancellationToken);

        if (tournament is null)
        {
            return null;
        }

        var result = new TournamentListItem(
            tournament.Id,
            tournament.Name,
            tournament.StartDate,
            tournament.EndDate,
            tournament.MaxTeams,
            tournament.PlayersOnFieldPerTeam,
            tournament.Status.ToString());

        await cacheService.SetAsync(cacheKey, result, CacheTtl, cancellationToken);

        return result;
    }
}